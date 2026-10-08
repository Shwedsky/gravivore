using System;
using System.Globalization;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    /// <summary>Bounded screen-space read model. No gameplay mutation or per-frame strings/searches.</summary>
    [DisallowMultipleComponent]
    public sealed class EnemyCombatReadabilityPresenter : MonoBehaviour
    {
        private sealed class Actor
        {
            public OrdinaryEnemyController Enemy;
            public Transform Root;
            public EnemyLifeId Life;
            public CoreReward Reward;
            public bool Strong;
            public float RecentUntil, Score;
            public Vector3 Anchor;
        }
        private sealed class Plate
        {
            public RectTransform Rect;
            public CanvasGroup Group;
            public HealthBarView Bar;
            public Text Health, Reward;
            public Image Edge;
            public Image Fill;
            public Actor Actor;
            public float LastHp = -1, LastMax = -1;
            public CoreReward LastReward;
            public EnemyLifeId Life;
        }
        private sealed class Floating
        {
            public RectTransform Rect;
            public Text Text;
            public CanvasGroup Group;
            public Vector3 Origin;
            public float Age, Lifetime;
            public bool Active;
            public float AggregatedDamage;
        }
        private static readonly Color Hostile = new Color(.95f,.25f,.23f,1);
        private static readonly Color Elite = new Color(1,.65f,.19f,1);
        private static readonly Color Player = new Color(.35f,.92f,.95f,1);
        private S01SceneCompositionRoot _root;
        private GravityAttackController _attack;
        private PostDevicePresentationDefinition _settings;
        private UnityEngine.Camera _camera;
        private RectTransform _layer;
        private CanvasGroup _layerGroup;
        private RectTransform _compactMap;
        private readonly Vector3[] _mapCorners = new Vector3[4];
        private Actor[] _actors;
        private int[] _order;
        private Plate[] _plates;
        private Floating[] _floating;
        private int _damageCursor, _rewardCursor, _incomingCursor;
        private int _lastIncoming=-1;
        public int IncomingFeedbackCount { get; private set; }
        public string LastIncomingText { get; private set; }
        public int PlateCapacity => _plates?.Length ?? 0;
        public int TextCapacity => _floating?.Length ?? 0;
        public int ActiveCombatTextCount { get; private set; }
        public int VisiblePlateCount { get; private set; }
        public int DamageFeedbackCount { get; private set; }
        public int RewardFeedbackCount { get; private set; }
        public float LastAppliedDamage { get; private set; }
        public string LastDamageText { get; private set; }
        public CoreReward LastGrantedReward { get; private set; }

        public void Initialize(S01SceneCompositionRoot root, PostDevicePresentationDefinition settings, RectTransform parent, UnityEngine.Camera camera)
        {
            if (_root != null) throw new InvalidOperationException("Combat readability already initialized.");
            settings.ValidateOrThrow(); _root = root; _settings = settings; _camera = camera;
            _attack = root.PlayerObject.GetComponent<GravityAttackController>();
            _layer = new GameObject("Enemy Combat Overlay",typeof(RectTransform)).GetComponent<RectTransform>();
            _layer.SetParent(parent,false); HudUiFactory.SetRect(_layer,Vector2.zero,Vector2.one);
            _layer.SetAsFirstSibling();
            _layerGroup = _layer.gameObject.AddComponent<CanvasGroup>();
            _layerGroup.blocksRaycasts=false; _layerGroup.interactable=false;
            _compactMap=root.MapIntegration.MapPresenter.CompactSurface.parent as RectTransform;
            var enemies = root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true);
            _actors = new Actor[enemies.Length+1]; _order = new int[_actors.Length];
            for (var i=0;i<enemies.Length;i++) _actors[i] = new Actor { Enemy=enemies[i],Root=enemies[i].transform };
            _actors[enemies.Length] = new Actor { Root=root.MagnetarGuard.transform,Strong=true };
            _plates = new Plate[settings.VisibleBars];
            for(var i=0;i<_plates.Length;i++) _plates[i]=CreatePlate(i);
            _floating = new Floating[settings.DamageTextCapacity+settings.RewardTextCapacity+3];
            for(var i=0;i<_floating.Length;i++) _floating[i]=CreateFloating(i);
            _attack.AttackResolved += AttackResolved;
            root.PlayerHealth.Damaged += IncomingDamaged;
            root.EnemyPopulation.EnemyDamaged += Damaged;
            root.Progression.RewardGranted += RewardGranted;
            root.Chapter1Encounters.Transactions.RewardApplied += EncounterReward;
        }
        private Plate CreatePlate(int i)
        {
            var rect=HudUiFactory.CreatePanel(_layer,"Enemy Plate "+i,Vector2.one*.5f,Vector2.one*.5f,HudUiFactory.PanelColor);
            rect.sizeDelta=_settings.PlateSize; var group=rect.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts=false; group.interactable=false;
            var edge=HudUiFactory.CreatePanel(rect,"Hostile Identity",Vector2.zero,new Vector2(.012f,1),Hostile).GetComponent<Image>();
            var bar=HealthBarView.Create(rect,"HP Track","HP Fill",new Vector2(.05f,.58f),new Vector2(.95f,.70f),new Color(.13f,.08f,.09f,1),Hostile);
            var hp=HudUiFactory.CreateText(rect,"HP",new Vector2(.05f,.70f),new Vector2(.95f,1),"",22,TextAnchor.MiddleCenter,Color.white);
            var reward=HudUiFactory.CreateText(rect,"Reward Preview",new Vector2(.04f,.03f),new Vector2(.96f,.57f),"",21,TextAnchor.MiddleCenter,Player);
            rect.gameObject.SetActive(false);
            return new Plate { Rect=rect,Group=group,Bar=bar,Health=hp,Reward=reward,Edge=edge,Fill=bar.FillRect.GetComponent<Image>() };
        }
        private Floating CreateFloating(int i)
        {
            var text=HudUiFactory.CreateText(_layer,"Pooled Combat Text "+i,Vector2.one*.5f,Vector2.one*.5f,"",36,TextAnchor.MiddleCenter,Color.white);
            text.rectTransform.sizeDelta=new Vector2(380,84); text.fontStyle=FontStyle.Bold;
            var outline=text.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(.01f,.02f,.025f,.95f); outline.effectDistance=new Vector2(2,-2);
            var group=text.gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts=false; group.interactable=false;
            text.gameObject.SetActive(false); return new Floating { Rect=text.rectTransform,Text=text,Group=group };
        }
        private void Damaged(EnemyDamageEvent value)
        {
            for(var i=0;i<_actors.Length;i++) if(_actors[i].Enemy!=null && _actors[i].Enemy.LifeId.Equals(value.LifeId))
            {
                if(!_actors[i].Life.Equals(value.LifeId)) BindLife(_actors[i]);
                _actors[i].RecentUntil=Time.unscaledTime+_settings.RecentDamageSeconds;
            }
        }
        public static string FormatDamage(float applied) => applied>0 ? applied.ToString("0.##",CultureInfo.InvariantCulture) : "БЛОК";
        public static string FormatReward(CoreReward reward) =>
            RussianUiText.StatName(reward.Stat)+" +"+reward.StatExperience.ToString("0.##",CultureInfo.InvariantCulture)+" ОП\nАссимиляция +"+reward.AssimilationScore;
        private void AttackResolved(PlayerAttackResolvedEvent value)
        {
            LastAppliedDamage=value.Result.AppliedDamage; LastDamageText=FormatDamage(LastAppliedDamage); DamageFeedbackCount++;
            ShowText(value.Position+Vector3.up*1.4f,LastDamageText,new Color(.80f,1f,1f,1),false);
        }
        private void IncomingDamaged(DamageResult value)
        {
            if (value.AppliedDamage <= 0) return;
            IncomingFeedbackCount++;
            var aggregate=_lastIncoming>=0 && _floating[_lastIncoming].Active && _floating[_lastIncoming].Age<.15f;
            var index = aggregate?_lastIncoming:_settings.DamageTextCapacity + _settings.RewardTextCapacity + _incomingCursor++ % 3;
            var slot = _floating[index];
            slot.AggregatedDamage=aggregate?slot.AggregatedDamage+value.AppliedDamage:value.AppliedDamage;
            _lastIncoming=index;LastIncomingText="−"+FormatDamage(slot.AggregatedDamage);
            slot.Origin = _root.PlayerObject.transform.position + Vector3.up * 1.8f+Vector3.right*((index%3-1)*.2f);
            slot.Age = 0; slot.Lifetime = _settings.DamageLifetime;
            slot.Text.fontSize = 42; slot.Text.text = LastIncomingText;
            slot.Text.color = new Color(1,.32f,.26f,1); slot.Active = true;
            slot.Group.alpha = 1; slot.Rect.gameObject.SetActive(true);
        }
        private void RewardGranted(CoreRewardGrantedEvent value)
        {
            var reward=new CoreReward(value.EnemyId,value.Stat,value.GrantedExperience,value.GrantedAssimilationScore);
            ShowReward(reward,value.WorldPosition);
        }
        private void EncounterReward(PendingEncounterReward value) => ShowReward(value.Reward,
            value.EncounterKind==RepeatableEncounterKind.Magnetar ? _root.MagnetarGuard.transform.position : _root.CustodianBoss.transform.position);
        private void ShowReward(CoreReward reward,Vector3 position)
        {
            LastGrantedReward=reward; RewardFeedbackCount++;
            ShowText(position+Vector3.up*1.8f,FormatReward(reward),Player,true);
        }
        private void ShowText(Vector3 origin,string text,Color color,bool reward)
        {
            var index=reward ? _settings.DamageTextCapacity+_rewardCursor++%_settings.RewardTextCapacity : _damageCursor++%_settings.DamageTextCapacity;
            var slot=_floating[index]; slot.Origin=origin; slot.Age=0; slot.Lifetime=reward?_settings.RewardLifetime:_settings.DamageLifetime;
            slot.Text.fontSize=reward?24:36;
            slot.Active=true; slot.Text.text=text; slot.Text.color=color; slot.Group.alpha=1; slot.Rect.gameObject.SetActive(true);
        }
        private void LateUpdate() => Tick(Time.unscaledDeltaTime);
        public void Tick(float dt)
        {
            if(_root==null) return;
            _layerGroup.alpha=(_root.PauseMenu.IsPaused || _root.OfflineRewardPanel.IsVisible || _root.MapIntegration.MapPresenter.IsExpanded)?0:1;
            var target=_attack.ValidCurrentTarget; var player=_root.PlayerObject.transform.position; var count=0;
            for(var i=0;i<_actors.Length;i++)
            {
                var actor=_actors[i];
                var alive=actor.Enemy!=null ? actor.Enemy.IsAlive : _root.MagnetarGuard.CanBeTargeted;
                if(!alive) continue;
                if(actor.Enemy!=null && !actor.Life.Equals(actor.Enemy.LifeId)) BindLife(actor);
                if(actor.Enemy==null) actor.Reward=_root.Chapter1Encounters.PreviewReward(RepeatableEncounterKind.Magnetar);
                var distance=(actor.Root.position-player).sqrMagnitude;
                var selected=ReferenceEquals(target,actor.Enemy!=null?(ITargetable)actor.Enemy:_root.MagnetarGuard);
                var recent=Time.unscaledTime<actor.RecentUntil;
                if(distance>_settings.RelevanceRange*_settings.RelevanceRange && !recent && !selected) continue;
                actor.Anchor=actor.Root.position+Vector3.up*(actor.Enemy!=null?actor.Enemy.TargetPoint.localPosition.y+1.05f:2.1f);
                if(!OnScreen(actor.Anchor,out _)) continue;
                actor.Score=distance-(selected?10000:recent?1000:actor.Strong?100:0);
                var j=count; while(j>0 && _actors[_order[j-1]].Score>actor.Score) { _order[j]=_order[j-1]; j--; }
                _order[j]=i; count++;
            }
            VisiblePlateCount=0;
            for(var i=0;i<count && VisiblePlateCount<_plates.Length;i++)
            {
                var actor=_actors[_order[i]]; OnScreen(actor.Anchor,out var point);
                point=KeepPlateClearOfMap(point);
                var overlaps=false;
                for(var j=0;j<VisiblePlateCount;j++)
                {
                    var delta=_plates[j].Rect.anchoredPosition-point;
                    if(Mathf.Abs(delta.x)<_settings.PlateSize.x+8 && Mathf.Abs(delta.y)<_settings.PlateSize.y+8) { overlaps=true; break; }
                }
                if(overlaps) continue;
                ApplyPlate(_plates[VisiblePlateCount++],actor,point);
            }
            for(var i=VisiblePlateCount;i<_plates.Length;i++) _plates[i].Rect.gameObject.SetActive(false);
            ActiveCombatTextCount=0;
            for(var i=0;i<_floating.Length;i++)
            {
                var slot=_floating[i]; if(!slot.Active) continue;
                slot.Age+=Mathf.Max(0,dt);
                if(slot.Age>=slot.Lifetime) { slot.Active=false; slot.Rect.gameObject.SetActive(false); continue; }
                ActiveCombatTextCount++;
                var fraction=slot.Age/slot.Lifetime;
                slot.Rect.localScale = Vector3.one * Mathf.Lerp(1.24f, 1f, Mathf.Clamp01(slot.Age / .14f));
                var onScreen=OnScreen(slot.Origin+Vector3.up*(_settings.TextTravel*fraction),out var point);
                slot.Rect.gameObject.SetActive(onScreen); slot.Rect.anchoredPosition=point;
                slot.Group.alpha=1-Mathf.InverseLerp(.55f,1,fraction);
            }
        }
        private bool OnScreen(Vector3 world,out Vector2 point)
        {
            var viewport=_camera.WorldToViewportPoint(world);
            point=new Vector2((viewport.x-.5f)*_layer.rect.width,(viewport.y-.5f)*_layer.rect.height);
            return viewport.z>0 && viewport.x>.06f && viewport.x<.94f && viewport.y>.14f && viewport.y<.84f;
        }
        private Vector2 KeepPlateClearOfMap(Vector2 point)
        {
            if(_compactMap==null || !_compactMap.gameObject.activeInHierarchy) return point;
            _compactMap.GetWorldCorners(_mapCorners);
            var low=_layer.InverseTransformPoint(_mapCorners[0]);
            var high=_layer.InverseTransformPoint(_mapCorners[2]);
            var half=_settings.PlateSize*.5f;
            if(point.x+half.x>low.x && point.x-half.x<high.x && point.y+half.y>low.y && point.y-half.y<high.y)
                point.x=Mathf.Max(-_layer.rect.width*.5f+half.x+8,low.x-half.x-8);
            return point;
        }
        private void BindLife(Actor actor)
        {
            actor.Life=actor.Enemy.LifeId; actor.RecentUntil=0;
            for(var s=0;s<_root.EnemyPopulation.SpotCount;s++)
            {
                var spot=_root.EnemyPopulation.GetSpot(s);
                for(var e=0;e<spot.LiveCount;e++)
                    if(ReferenceEquals(spot.GetLiveEnemy(e),actor.Enemy))
                    {
                        _root.Progression.TryPreview(spot.EnemyId,spot.RewardMultiplier,out actor.Reward);
                        actor.Strong=spot.RewardMultiplier>1; return;
                    }
            }
        }
        private void ApplyPlate(Plate plate,Actor actor,Vector2 point)
        {
            var elite=actor.Enemy==null; var boss=actor.Root==_root.CustodianBoss.transform;
            var hp=actor.Enemy!=null?actor.Enemy.CurrentHitPoints:boss?_root.CustodianBoss.CurrentHitPoints:_root.MagnetarGuard.CurrentHitPoints;
            var max=actor.Enemy!=null?actor.Enemy.MaximumHitPoints:boss?_root.CustodianBoss.MaximumHitPoints:_root.MagnetarGuard.MaximumHitPoints;
            var changed=plate.Actor!=actor || !plate.Life.Equals(actor.Life);
            plate.Rect.gameObject.SetActive(true); plate.Rect.anchoredPosition=point;
            plate.Group.alpha=1; plate.Bar.SetNormalizedValue(max>0?hp/max:0);
            plate.Edge.color=elite||actor.Strong?Elite:Hostile;
            plate.Fill.color=elite||actor.Strong?Elite:Hostile;
            if(changed || plate.LastHp!=hp || plate.LastMax!=max)
                plate.Health.text=hp.ToString("0.#",CultureInfo.InvariantCulture)+" / "+max.ToString("0.#",CultureInfo.InvariantCulture);
            if(changed || plate.LastReward.StatExperience!=actor.Reward.StatExperience || plate.LastReward.AssimilationScore!=actor.Reward.AssimilationScore)
                plate.Reward.text=FormatReward(actor.Reward);
            // Custodian already has the persistent authoritative numeric boss HUD.
            plate.Health.gameObject.SetActive(!boss); plate.Bar.FillRect.parent.gameObject.SetActive(!boss);
            plate.Actor=actor; plate.Life=actor.Life; plate.LastHp=hp; plate.LastMax=max; plate.LastReward=actor.Reward;
        }
        public bool TryReadPlate(OrdinaryEnemyController enemy,out float hpFraction,out string reward)
        {
            for(var i=0;i<VisiblePlateCount;i++) if(_plates[i].Actor.Enemy==enemy)
            { hpFraction=_plates[i].Bar.NormalizedValue; reward=_plates[i].Reward.text; return true; }
            hpFraction=0; reward=null; return false;
        }
        private void OnDisable()
        {
            if(_floating==null) return;
            foreach(var slot in _floating) { slot.Active=false; slot.Rect.gameObject.SetActive(false); }
            foreach(var plate in _plates) plate.Rect.gameObject.SetActive(false);
            ActiveCombatTextCount=VisiblePlateCount=0;
        }
        private void OnDestroy()
        {
            if(_attack!=null) _attack.AttackResolved-=AttackResolved;
            if(_root==null) return;
            if(_root.PlayerHealth!=null) _root.PlayerHealth.Damaged-=IncomingDamaged;
            if(_root.EnemyPopulation!=null) _root.EnemyPopulation.EnemyDamaged-=Damaged;
            if(_root.Progression!=null) _root.Progression.RewardGranted-=RewardGranted;
            if(_root.Chapter1Encounters!=null) _root.Chapter1Encounters.Transactions.RewardApplied-=EncounterReward;
        }
    }
}
