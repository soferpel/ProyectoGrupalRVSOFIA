//using Unity.Netcode;
//using UnityEngine;

//public class MeshSyncManager : NetworkBehaviour
//{
//    public static MeshSyncManager Instance { get; private set; }

//    public NetworkList<MeshData> meshList;  // Aquí se almacenan las mallas para sincronizar

//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//        }
//        else
//        {
//            Debug.LogError("Ya existe una instancia de MeshSyncManager en la escena.");
//            Destroy(gameObject);
//        }
//        meshList = new NetworkList<MeshData>();
//    }

//    public override void OnNetworkSpawn()
//    {
//        if (IsClient)
//        {
//            meshList.OnListChanged += OnMeshAdded; // Nos suscribimos al evento de cambio de lista
//        }
//    }

//    private void OnDestroy()
//    {
//        if (meshList != null)
//            meshList.OnListChanged -= OnMeshAdded; // Nos desuscribimos al evento
//    }

//    // Este método se llama para agregar una nueva malla cortada
//    public void AddCutMesh(Mesh mesh, int materialId)
//    {
//        string json = JsonUtility.ToJson(MeshDataSerializable.FromMesh(mesh));  // Convertimos la malla a JSON
//        meshList.Add(new MeshData(json, materialId));  // Agregamos la malla a la lista
//    }

//    // Este método se llama cuando se añade un nuevo MeshData a la lista en cualquier cliente
//    private void OnMeshAdded(NetworkListEvent<MeshData> evt)
//    {
//        if (evt.Type != NetworkListEvent<MeshData>.EventType.Add) return;

//        var data = evt.Value;  // Obtenemos los datos de la malla
//        var meshData = JsonUtility.FromJson<MeshDataSerializable>(data.jsonMesh.ToString());  // Convertimos el JSON a MeshDataSerializable
//        Mesh mesh = meshData.ToMesh();  // Convertimos los datos serializados a una malla
//        Material mat = MaterialRegistry.GetMaterial(data.materialId);  // Obtenemos el material usando el ID

//        // Creamos un nuevo GameObject para la malla
//        GameObject go = new GameObject("SyncedPart");
//        var mf = go.AddComponent<MeshFilter>();
//        mf.sharedMesh = mesh;

//        var mr = go.AddComponent<MeshRenderer>();
//        mr.sharedMaterial = mat;

//        var mc = go.AddComponent<MeshCollider>();
//        mc.sharedMesh = mesh;
//        mc.convex = true;
//    }
//}
