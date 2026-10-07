using System;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    /// <summary>Event driven, preallocated rows. Shares the existing menu's pause/input ownership.</summary>
    public sealed class CharacteristicsPresenter : MonoBehaviour
    {
        private sealed class Row { public PlayerStatType Stat; public Text Title, Progress, Effect, Description; public HealthBarView Bar; }
        private PlayerStatsState _stats;
        private AssimilationProgressionService _progression;
        private WorldUnlockState _world;
        private QuestService _quests;
        private Row[] _rows;
        private RectTransform _panel, _detail;
        private Text _assimilation, _assimilationExplanation, _detailText;
        private Action _closed;
        private PlayerStatType _selected;
        public CharacteristicsReadModel Model { get; private set; }
        public bool IsVisible => _panel != null && _panel.gameObject.activeSelf;
        public RectTransform Panel => _panel;
        public string AssimilationText => _assimilation?.text;
        public int Revision { get; private set; }
        public string ReadRow(PlayerStatType stat) => _rows[(int)stat].Progress.text;

        public void Initialize(RectTransform menuRoot, PlayerStatsState stats, AssimilationProgressionService progression,
            EliteGateRequirement requirement, QuestService quests, WorldUnlockState world, Action closed)
        {
            _stats=stats; _progression=progression; _world=world; _quests=quests; _closed=closed;
            Model=new CharacteristicsReadModel(stats,progression,requirement,quests.State,world);
            _panel=HudUiFactory.CreatePanel(menuRoot,"Characteristics Screen",new Vector2(.07f,.08f),new Vector2(.93f,.92f),
                new Color(.026f,.045f,.057f,1),true);
            HudUiFactory.CreateText(_panel,"Title",new Vector2(.06f,.93f),new Vector2(.94f,.985f),"ХАРАКТЕРИСТИКИ",36,TextAnchor.MiddleLeft,Color.white);
            HudUiFactory.CreateText(_panel,"Explanation",new Vector2(.06f,.88f),new Vector2(.94f,.93f),
                "Поглощайте ядра: ОП повышают уровень. Нажмите на характеристику.",23,TextAnchor.MiddleLeft,new Color(.65f,.76f,.8f,1));
            var viewport=HudUiFactory.CreatePanel(_panel,"Stat Viewport",new Vector2(.05f,.245f),new Vector2(.95f,.88f),Color.clear,true);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content=new GameObject("Stat Rows",typeof(RectTransform)).GetComponent<RectTransform>(); content.SetParent(viewport,false);
            content.anchorMin=new Vector2(0,1); content.anchorMax=Vector2.one; content.pivot=new Vector2(.5f,1);
            var types=(PlayerStatType[])Enum.GetValues(typeof(PlayerStatType));
            content.sizeDelta=new Vector2(0,types.Length*184); content.anchoredPosition=Vector2.zero;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>(); scroll.viewport=viewport; scroll.content=content;
            scroll.horizontal=false; scroll.movementType=ScrollRect.MovementType.Clamped;
            _rows=new Row[types.Length];
            for(var i=0;i<types.Length;i++)
            {
                var stat=types[i];
                var rect=HudUiFactory.CreatePanel(content,"Characteristic "+stat,new Vector2(0,1),Vector2.one,new Color(.055f,.083f,.097f,1),true);
                rect.pivot=new Vector2(.5f,1); rect.sizeDelta=new Vector2(0,172); rect.anchoredPosition=new Vector2(0,-i*184);
                var button=rect.gameObject.AddComponent<Button>(); button.onClick.AddListener(()=>Select(stat));
                HudUiFactory.CreatePanel(rect,"Identity",Vector2.zero,new Vector2(.006f,1),HudUiFactory.AccentColor);
                var row=new Row { Stat=stat };
                row.Title=HudUiFactory.CreateText(rect,"Stat Level",new Vector2(.04f,.70f),new Vector2(.96f,.99f),"",27,TextAnchor.MiddleLeft,Color.white);
                row.Effect=HudUiFactory.CreateText(rect,"Actual Effect",new Vector2(.04f,.50f),new Vector2(.96f,.72f),"",24,TextAnchor.MiddleLeft,HudUiFactory.AccentColor);
                row.Description=HudUiFactory.CreateText(rect,"Description",new Vector2(.04f,.18f),new Vector2(.96f,.49f),CharacteristicsReadModel.Description(stat),22,TextAnchor.MiddleLeft,new Color(.68f,.77f,.81f,1));
                row.Bar=HealthBarView.Create(rect,"XP Track","XP Fill",new Vector2(.04f,.055f),new Vector2(.65f,.12f),new Color(.015f,.025f,.033f,1),HudUiFactory.AccentColor);
                row.Progress=HudUiFactory.CreateText(rect,"Actual XP",new Vector2(.67f,.015f),new Vector2(.96f,.19f),"",22,TextAnchor.MiddleRight,Color.white);
                _rows[i]=row;
            }
            _assimilation=HudUiFactory.CreateText(_panel,"Assimilation Gate Progress",new Vector2(.06f,.15f),new Vector2(.94f,.245f),"",26,TextAnchor.MiddleLeft,new Color(1,.72f,.3f,1));
            _assimilationExplanation=HudUiFactory.CreateText(_panel,"Assimilation Explanation",new Vector2(.06f,.065f),new Vector2(.94f,.15f),"",22,TextAnchor.MiddleLeft,new Color(.68f,.77f,.81f,1));
            HudUiFactory.CreateButton(_panel,"Back To Menu",new Vector2(.60f,.01f),new Vector2(.94f,.06f),"НАЗАД",Close);
            _detail=HudUiFactory.CreatePanel(_panel,"Characteristic Detail",new Vector2(.02f,.07f),new Vector2(.98f,.92f),new Color(.018f,.032f,.043f,1),true);
            _detailText=HudUiFactory.CreateText(_detail,"Detail Text",new Vector2(.08f,.18f),new Vector2(.92f,.94f),"",29,TextAnchor.UpperLeft,Color.white);
            HudUiFactory.CreateButton(_detail,"Close Detail",new Vector2(.55f,.04f),new Vector2(.92f,.13f),"НАЗАД",()=>_detail.gameObject.SetActive(false));
            _detail.gameObject.SetActive(false); _panel.gameObject.SetActive(false);
            stats.DerivedStatsChanged+=Derived; stats.StatChanged+=Level;
            progression.Dirty+=Dirty; world.GateUnlocked+=Gate; quests.ObjectiveCompleted+=Objective;
            Refresh();
        }
        public void Open() { Refresh(); _panel.SetAsLastSibling(); _panel.gameObject.SetActive(true); }
        public void Close() { _detail.gameObject.SetActive(false); _panel.gameObject.SetActive(false); _closed?.Invoke(); }
        public void Select(PlayerStatType stat) { _selected=stat; _detail.gameObject.SetActive(true); ApplyDetail(); }
        public void Refresh()
        {
            if(Model==null) return; Revision++;
            foreach(var row in _rows)
            {
                var value=Model.Read(row.Stat);
                row.Title.text=$"{RussianUiText.StatName(row.Stat).ToUpperInvariant()}   •   Уровень {value.Level}";
                row.Progress.text=value.IsMaximum?"ПРЕДЕЛ":$"{CharacteristicsReadModel.FormatNumber(value.Experience)} / {CharacteristicsReadModel.FormatNumber(value.Required)} ОП";
                row.Bar.SetNormalizedValue(value.Fraction);
                row.Effect.text=CharacteristicsReadModel.Effect(row.Stat,value.Current) +
                    (value.IsMaximum?"":$" → {CharacteristicsReadModel.FormatNumber(CharacteristicsReadModel.Value(row.Stat,value.Next))}");
            }
            _assimilation.text=Model.AssimilationText;
            _assimilationExplanation.text=Model.AssimilationExplanation;
            if(_detail.gameObject.activeSelf) ApplyDetail();
        }
        private void ApplyDetail()
        {
            var value=Model.Read(_selected);
            _detailText.text=$"{RussianUiText.StatName(_selected).ToUpperInvariant()}\n\n{CharacteristicsReadModel.Description(_selected)}\n\n"+
                $"Уровень: {value.Level}\n{CharacteristicsReadModel.Effect(_selected,value.Current)}\n"+
                (value.IsMaximum?"Достигнут предел уровня.":$"До уровня {value.Level+1}: {CharacteristicsReadModel.FormatNumber(value.Experience)} / {CharacteristicsReadModel.FormatNumber(value.Required)} ОП\nСледующий уровень — {CharacteristicsReadModel.Effect(_selected,value.Next)}")+
                "\n\n"+Model.Sources(_selected);
        }
        private void Derived(PlayerDerivedStatsChange _) => Refresh();
        private void Level(PlayerStatChange _) => Refresh();
        private void Dirty(ProgressionDirtyEvent _) => Refresh();
        private void Gate(WorldGateUnlockedEvent _) => Refresh();
        private void Objective(QuestObjectiveCompletedEvent _) => Refresh();
        private void OnDestroy()
        {
            if(_stats==null) return;
            _stats.DerivedStatsChanged-=Derived; _stats.StatChanged-=Level; _progression.Dirty-=Dirty;
            _world.GateUnlocked-=Gate; _quests.ObjectiveCompleted-=Objective;
        }
    }
}
