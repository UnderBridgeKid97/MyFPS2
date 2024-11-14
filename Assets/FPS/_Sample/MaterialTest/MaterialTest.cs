using UnityEngine;

namespace MySample
{

    public class MaterialTest : MonoBehaviour
    {
        #region Variables

        private Renderer _renderer;
        private MaterialPropertyBlock materialPropertyBlock;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // 참조
            _renderer = GetComponent<Renderer>();

            // 메테리얼 컬러 바꾸기
          //  renderer.material.SetColor("_BaseColor", Color.red);
         // renderer.sharedMaterial.SetColor("_BaseColor", Color.red);

            materialPropertyBlock = new MaterialPropertyBlock();
            materialPropertyBlock.SetColor("_BaseColor", Color.red);
            _renderer.SetPropertyBlock(materialPropertyBlock);

        }


        
    }
}