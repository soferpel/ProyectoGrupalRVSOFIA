//using UnityEngine;
//using System.Collections.Generic;

//public static class MaterialRegistry
//{
//    private static Dictionary<int, Material> materialRegistry = new Dictionary<int, Material>();
//    private static int nextMaterialId = 0;

//    // Registra un material y devuelve su ID
//    public static int Register(Material material)
//    {
//        int id = nextMaterialId++;
//        materialRegistry.Add(id, material);
//        return id;
//    }

//    // Obtiene el material registrado por su ID
//    public static Material GetMaterial(int id)
//    {
//        if (materialRegistry.TryGetValue(id, out Material material))
//        {
//            return material;
//        }
//        return null;
//    }
//}
