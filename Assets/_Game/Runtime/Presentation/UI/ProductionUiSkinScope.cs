using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    /// <summary>Skin injected on the UI hierarchy; no global mutable skin or scene search.</summary>
    public sealed class ProductionUiSkinScope : MonoBehaviour
    {
        public ProductionUiSkinDefinition Definition { get; private set; }
        public void Initialize(ProductionUiSkinDefinition definition) => Definition=definition;
        internal static void Frame(RectTransform rect, Color surface)
        {
            var scope=rect.GetComponentInParent<ProductionUiSkinScope>();
            if(scope?.Definition==null || surface.a<.1f || rect.name.IndexOf("Fill",StringComparison.OrdinalIgnoreCase)>=0 || rect.name.IndexOf("Backdrop",StringComparison.OrdinalIgnoreCase)>=0) return;
            var image=rect.GetComponent<Image>();
            if(rect.name.IndexOf("Track",StringComparison.OrdinalIgnoreCase)>=0)
            {image.sprite=scope.Definition.Utility;image.type=Image.Type.Sliced;return;}
            var go=new GameObject("Production EXE Frame",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            var frame=go.GetComponent<RectTransform>();frame.SetParent(rect,false);HudUiFactory.SetRect(frame,Vector2.zero,Vector2.one);
            var border=go.GetComponent<Image>();border.sprite=scope.Definition.Frame;border.type=Image.Type.Sliced;border.fillCenter=false;
            border.color=rect.name.IndexOf("Boss",StringComparison.OrdinalIgnoreCase)>=0?new Color(.78f,.32f,.14f,.80f):new Color(.40f,.60f,.68f,.65f);
            border.raycastTarget=false;
            if(scope.Definition.Divider!=null)
            {
                var rail=new GameObject("Production EXE Status Rail",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
                var railRect=rail.GetComponent<RectTransform>();railRect.SetParent(rect,false);
                HudUiFactory.SetRect(railRect,new Vector2(.05f,.985f),new Vector2(.27f,.99f));
                var railImage=rail.GetComponent<Image>();railImage.sprite=scope.Definition.Divider;railImage.color=border.color;railImage.raycastTarget=false;
            }
        }
    }
}
