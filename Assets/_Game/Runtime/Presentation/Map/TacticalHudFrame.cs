using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Map
{
    /// <summary>Vector HUD frame; no texture, camera or extra runtime materials.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TacticalHudFrame : MaskableGraphic
    {
        [SerializeField] private bool _border;
        public void Configure(bool border) { _border=border; raycastTarget=false; SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var cut=Mathf.Min(r.width,r.height)*.075f;
            var center=r.center;
            for(var i=0;i<8;i++)
            {
                var a=Corner(r,cut,i);var b=Corner(r,cut,(i+1)%8);
                if(_border)
                {var n=(center-(a+b)*.5f).normalized*1.5f;TacticalMapGraphic.Quad(vh,a,b,b+n,a+n,new Color(.32f,.70f,.77f,.9f));}
                else
                {var k=vh.currentVertCount;vh.AddVert(center,color,Vector2.zero);vh.AddVert(a,color,Vector2.zero);vh.AddVert(b,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);}
            }
        }
        private static Vector2 Corner(Rect r,float c,int i)
        {
            switch(i)
            {
                case 0:return new Vector2(r.xMin+c,r.yMin);case 1:return new Vector2(r.xMax-c,r.yMin);
                case 2:return new Vector2(r.xMax,r.yMin+c);case 3:return new Vector2(r.xMax,r.yMax-c);
                case 4:return new Vector2(r.xMax-c,r.yMax);case 5:return new Vector2(r.xMin+c,r.yMax);
                case 6:return new Vector2(r.xMin,r.yMax-c);default:return new Vector2(r.xMin,r.yMin+c);
            }
        }
    }
}
