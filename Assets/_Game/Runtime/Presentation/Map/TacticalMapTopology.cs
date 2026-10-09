using System;
using System.Collections.Generic;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.World;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.Map
{
    /// <summary>Read-only schematic of the same boxes that stop the CharacterController.</summary>
    public sealed class TacticalMapTopology
    {
        public readonly struct Footprint
        {
            public Footprint(Collider collider)
            {
                Authority = collider;
                var box = collider as BoxCollider;
                if (box == null) throw new ArgumentException("Tactical blockers require authored box geometry.");
                var c = box.center; var h = box.size * .5f; var t = box.transform;
                A = t.TransformPoint(c + new Vector3(-h.x, 0, -h.z));
                B = t.TransformPoint(c + new Vector3(h.x, 0, -h.z));
                C = t.TransformPoint(c + new Vector3(h.x, 0, h.z));
                D = t.TransformPoint(c + new Vector3(-h.x, 0, h.z));
            }
            public Collider Authority { get; }
            public Vector3 A { get; } public Vector3 B { get; } public Vector3 C { get; } public Vector3 D { get; }
            public bool Blocks => Authority != null && Authority.enabled && Authority.gameObject.activeInHierarchy;
        }
        public readonly struct Area
        {
            public Area(Vector3 center, Vector2 size, Color tint) { Center = center; Size = size; Tint = tint; }
            public Vector3 Center { get; } public Vector2 Size { get; } public Color Tint { get; }
        }
        private readonly Footprint[] _blockers;
        private readonly Area[] _areas;
        private readonly Area[] _surfaces;
        public TacticalMapTopology(Chapter01WorldPresenter world, EnemyPopulationController population, Vector3 repair, float repairRadius)
        {
            Bounds = new MapWorldBounds(world.Bounds.MinX, world.Bounds.MaxX, world.Bounds.MinZ, world.Bounds.MaxZ);
            var walls = new List<Footprint>();
            foreach (var collider in world.GameplayRoot.GetComponentsInChildren<BoxCollider>(true))
                if (collider.gameObject.layer == LayerMask.NameToLayer("HardBlocker")) walls.Add(new Footprint(collider));
            _blockers = walls.ToArray();
            var surfaces = new List<Area>();
            if (world.Layout != null)
                for (var i = 0; i < world.Layout.SurfaceCount; i++)
                {
                    var surface = world.Layout.GetSurface(i);
                    surfaces.Add(new Area(surface.Center, new Vector2(surface.Size.x, surface.Size.z), new Color(.08f,.17f,.21f,.92f)));
                }
            else surfaces.Add(new Area(new Vector3(Bounds.Center.x,0,Bounds.Center.y),new Vector2(Bounds.Width,Bounds.Depth),new Color(.08f,.17f,.21f,.92f)));
            _surfaces = surfaces.ToArray();
            var areas = new List<Area>();
            for (var i = 0; i < population.SpotCount; i++)
            {
                var spot = population.GetSpot(i);
                areas.Add(new Area(spot.Position, spot.WaveFootprintSize, new Color(.20f,.36f,.40f,.65f)));
            }
            areas.Add(new Area(repair, Vector2.one * repairRadius * 2f, new Color(.12f,.43f,.43f,.8f)));
            var config = world.Configuration;
            areas.Add(new Area(config.BossArenaCenter, Vector2.one * config.BossArenaRadius * 2f, new Color(.38f,.19f,.24f,.7f)));
            areas.Add(new Area((config.EliteGate.Position + config.BossGate.Position) * .5f,
                new Vector2(config.EliteGate.Size.x, config.BossGate.Position.z - config.EliteGate.Position.z), new Color(.32f,.30f,.19f,.6f)));
            _areas = areas.ToArray();
            EliteGate = world.EliteGate; BossGate = world.BossGate;
        }
        public MapWorldBounds Bounds { get; }
        public WorldGateView EliteGate { get; } public WorldGateView BossGate { get; }
        public int BlockerCount => _blockers.Length;
        public int SurfaceCount => _surfaces.Length;
        public Area GetSurface(int index) => _surfaces[index];
        public Footprint GetBlocker(int index) => _blockers[index];
        public int AreaCount => _areas.Length;
        public Area GetArea(int index) => _areas[index];
    }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TacticalMapGraphic : MaskableGraphic
    {
        private TacticalMapTopology _topology;
        private MapWorldBounds _window;
        private bool _eliteLocked, _bossLocked;
        public void Initialize(TacticalMapTopology topology) { _topology = topology; raycastTarget = false; Refresh(topology.Bounds); }
        public void Refresh(MapWorldBounds window)
        {
            var elite = _topology.EliteGate.IsLocked; var boss = _topology.BossGate.IsLocked;
            if (_window.Equals(window) && elite == _eliteLocked && boss == _bossLocked) return;
            _window = window; _eliteLocked = elite; _bossLocked = boss; SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (_topology == null) return;
            var bounds = _topology.Bounds;
            for (var i=0;i<_topology.SurfaceCount;i++)
            { var surface=_topology.GetSurface(i); Rectangle(vh,surface.Center,surface.Size,surface.Tint); }
            // Open floor is traversable; solid authority footprints carve the actual corridors.
            for (var i=0;i<_topology.AreaCount;i++)
            { var area = _topology.GetArea(i); Rectangle(vh,area.Center,area.Size,area.Tint); }
            for (var i=0;i<_topology.BlockerCount;i++)
            {
                var wall = _topology.GetBlocker(i);
                var gate = wall.Authority == _topology.EliteGate.BlockingCollider || wall.Authority == _topology.BossGate.BlockingCollider;
                var tint = gate ? wall.Blocks ? new Color(.96f,.49f,.20f,1) : new Color(.15f,.78f,.70f,.55f) : new Color(.04f,.08f,.11f,1);
                if (!wall.Blocks && !gate) continue;
                if (wall.Blocks) Quad(vh,Project(wall.A),Project(wall.B),Project(wall.C),Project(wall.D),tint);
                var edge = gate ? tint : new Color(.36f,.55f,.62f,.95f);
                Line(vh,Project(wall.A),Project(wall.B),edge); Line(vh,Project(wall.B),Project(wall.C),edge);
                Line(vh,Project(wall.C),Project(wall.D),edge); Line(vh,Project(wall.D),Project(wall.A),edge);
            }
        }
        private Vector2 Project(Vector3 point)
        {
            var r = rectTransform.rect;
            return new Vector2(r.xMin+(point.x-_window.MinX)/_window.Width*r.width, r.yMin+(point.z-_window.MinZ)/_window.Depth*r.height);
        }
        private void Rectangle(VertexHelper vh, Vector3 center, Vector2 size, Color tint)
        {
            var h=size*.5f;
            Quad(vh,Project(center+new Vector3(-h.x,0,-h.y)),Project(center+new Vector3(h.x,0,-h.y)),
                Project(center+new Vector3(h.x,0,h.y)),Project(center+new Vector3(-h.x,0,h.y)),tint);
        }
        private static void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint)
        {
            var d=b-a;if(d.sqrMagnitude<.0001f)return;
            var n=new Vector2(-d.y,d.x).normalized*.8f;
            Quad(vh,a-n,b-n,b+n,a+n,tint);
        }
        internal static void Quad(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color tint)
        {
            var i=vh.currentVertCount;
            vh.AddVert(a,tint,Vector2.zero); vh.AddVert(b,tint,Vector2.zero); vh.AddVert(c,tint,Vector2.zero); vh.AddVert(d,tint,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2); vh.AddTriangle(i,i+2,i+3);
        }
    }
}
