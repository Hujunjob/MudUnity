using DefaultNamespace;
using IWorld.ContractDefinition;
using mud.Client;
using mud.Unity;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using ObservableExtensions = UniRx.ObservableExtensions;

public class PlayerManager : MonoBehaviour
{
	private CompositeDisposable _disposers = new();
	public GameObject playerPrefab;
	public Scrollbar scrollbar;
	public Text walletText;
	private NetworkManager net;


	// Start is called before the first frame update
	void Start()
	{
		net = NetworkManager.Instance;
		net.OnNetworkInitialized += Spawn;
		walletText.text = net.address;
		// Observable.Timer(System.TimeSpan.FromSeconds(5)).Subscribe(_=>{

		// }).AddTo(this);
	}

	public void CopyWallet(){
		string textToCopy = walletText.text;
		UnityEngine.GUIUtility.systemCopyBuffer = textToCopy;
	}

	private void FixedUpdate() {
		scrollbar.size = scrollbar.size + 0.0014f;
	}

	async void Spawn(NetworkManager nm)
	{
		scrollbar.gameObject.SetActive(false);
		Debug.Log("Spawn OnNetworkInitialized");
		// TODO: Check if current player exists in PlayerTable 
		var addressKey = net.addressKey;
		var currentPlayer = PlayerTable.GetTableValue(addressKey);
		// TODO: If not, make the Spawn Tx 
		if(currentPlayer!=null){

		}else{
			Debug.Log("Spawn player");
			await nm.worldSend.TxExecute<SpawnFunction>(0,0);
		}

		// TODO: Subscribe to PlayerTable
		ObservableExtensions.Subscribe(PlayerTable.OnRecordInsert().ObserveOnMainThread(),OnUpdatePlayers).AddTo(_disposers);
	}

	// TODO: Callback for PlayerTable update
	private void OnUpdatePlayers(PlayerTableUpdate update)
	{
		Debug.Log("OnUpdatePlayers");
		var currentValue = update.TypedValue.Item1;
		if(currentValue==null){
			return;
		}
		var playerPosition = PositionTable.GetTableValue(update.Key);
		if(playerPosition==null){
			return;
		}
		var plyaerP = new Vector3((float)playerPosition.x,0,(float)playerPosition.y);
		var player = Instantiate(playerPrefab,plyaerP,Quaternion.identity);

		player.GetComponent<PlayerSync>().key = update.Key;

		var cameraControl = GameObject.Find("CameraRig").GetComponent<CameraControl>();
		cameraControl.m_Targets.Add(player.transform);

		if(update.Key!=net.addressKey){return;}

		PlayerSync.localPlayerKey = update.Key;
	}

	private void OnDestroy()
	{
		_disposers?.Dispose();
	}
}
