using DiceRollerPro.Models;
using UnityEditor;
using UnityEngine;

namespace DiceRollerPro.Editor
{
    class DiceCreator
    {
        [MenuItem("Assets/Create/Dice Roller Pro/Fate Dice")]
        public static void CreateFateDice()
        {
            var dice = ScriptableObject.CreateInstance<FateDice>();
            CreateAsset(dice, "Fate Dice");
        }

        [MenuItem("Assets/Create/Dice Roller Pro/Group")]
        public static void CreateGroup()
        {
            var dice = ScriptableObject.CreateInstance<Group>();
            CreateAsset(dice, "Group");
        }

        [MenuItem("Assets/Create/Dice Roller Pro/Normal Dice")]
        public static void CreateNormalDice()
        {
            var dice = ScriptableObject.CreateInstance<NormalDice>();
            CreateAsset(dice, "Normal Dice");
        }

        [MenuItem("Assets/Create/Dice Roller Pro/Number")]
        public static void CreateNumber()
        {
            var dice = ScriptableObject.CreateInstance<Number>();
            CreateAsset(dice, "Number");
        }

        [MenuItem("Assets/Create/Dice Roller Pro/Sequence")]
        public static void CreateSequence()
        {
            var dice = ScriptableObject.CreateInstance<Sequence>();
            CreateAsset(dice, "Sequence");
        }

        private static void CreateAsset(ScriptableObject scriptableObject, string assetName)
        {
            var parentPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            var assetPath = parentPath + 
                            $"/{assetName}.asset";
                            //$"Assets/{Selection.activeObject.name}/{assetName}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            AssetDatabase.CreateAsset(scriptableObject, assetPath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = scriptableObject;
        }
    }
}