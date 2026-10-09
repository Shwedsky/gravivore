using Gravivore.Gameplay.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    public sealed class WeaponEquipmentPanel : MonoBehaviour
    {
        private EquipmentService _service;
        private RectTransform _panel, _toast, _menu;
        private Text _description, _toastText;
        private Button _toggle;
        private Image _model;
        private ProductionUiSkinDefinition _skin;
        private Text _selection;
        private Image _selectionFrame;
        public Sprite DisplayedWeapon => _model != null ? _model.sprite : null;
        private float _toastRemaining;
        public bool IsVisible => _panel != null && _panel.gameObject.activeSelf;
        public string Description => _description.text;
        public void Initialize(RectTransform menuRoot, RectTransform hudRoot, EquipmentService service, ProductionUiSkinDefinition skin=null)
        {
            _service = service;
            _skin=skin;
            var menu = (RectTransform)menuRoot.Find("Game Menu");
            _menu=menu;
            menu.Find("Equipment Title").gameObject.SetActive(false);
            HudUiFactory.CreateButton(menu, "Open Equipment", new Vector2(.08f,.42f), new Vector2(.92f,.49f), "СНАРЯЖЕНИЕ", Open);
            _panel = HudUiFactory.CreatePanel(menuRoot, "Equipment Panel", new Vector2(.07f,.08f), new Vector2(.93f,.92f), HudUiFactory.PanelColor, true);
            HudUiFactory.CreateText(_panel, "Title", new Vector2(.07f,.87f),new Vector2(.93f,.97f),"ОРУЖИЕ",34,TextAnchor.MiddleCenter,Color.white);
            HudUiFactory.CreateText(_panel, "Item", new Vector2(.08f,.68f),new Vector2(.92f,.86f),"ИМПУЛЬСНЫЙ ИЗЛУЧАТЕЛЬ М-0",30,TextAnchor.MiddleCenter,HudUiFactory.AccentColor);
            var modelRect=HudUiFactory.CreatePanel(_panel,"Actual Weapon Model",new Vector2(.08f,.50f),new Vector2(.92f,.69f),new Color(.03f,.05f,.06f,.8f));
            _selectionFrame=modelRect.Find("Production EXE Frame")?.GetComponent<Image>();
            var preview=new GameObject("Model Render",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            var previewRect=preview.GetComponent<RectTransform>();previewRect.SetParent(modelRect,false);HudUiFactory.SetRect(previewRect,new Vector2(.05f,.07f),new Vector2(.95f,.93f));
            _model=preview.GetComponent<Image>();_model.preserveAspect=true;_model.raycastTarget=false;
            _selection=HudUiFactory.CreateText(_panel,"Selected Equipped State",new Vector2(.08f,.45f),new Vector2(.92f,.50f),"",22,TextAnchor.MiddleCenter,HudUiFactory.AccentColor);
            _description = HudUiFactory.CreateText(_panel,"Description",new Vector2(.08f,.30f),new Vector2(.92f,.45f),"",24,TextAnchor.UpperLeft,Color.white);
            _toggle = HudUiFactory.CreateButton(_panel,"Equip Weapon",new Vector2(.08f,.18f),new Vector2(.92f,.28f),"",ToggleWeapon);
            HudUiFactory.CreateButton(_panel,"Back",new Vector2(.08f,.05f),new Vector2(.92f,.14f),"НАЗАД",Close);
            _panel.gameObject.SetActive(false);
            _toast = HudUiFactory.CreatePanel(hudRoot,"Weapon Loot Toast",new Vector2(.08f,.50f),new Vector2(.92f,.62f),HudUiFactory.PanelColor);
            _toastText = HudUiFactory.CreateText(_toast,"Loot",new Vector2(.04f,.06f),new Vector2(.96f,.94f),"",26,TextAnchor.MiddleCenter,HudUiFactory.AccentColor);
            _toast.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false; _toast.gameObject.SetActive(false);
            service.InventoryChanged += InventoryChanged; service.EquipmentGranted += Granted;
            Refresh();
        }
        public void Open() { Refresh(); _menu.gameObject.SetActive(false); _panel.SetAsLastSibling(); _panel.gameObject.SetActive(true); }
        public void Close() { if (_panel != null) _panel.gameObject.SetActive(false);if(_menu!=null)_menu.gameObject.SetActive(true); }
        public void ToggleWeapon()
        {
            if (!_service.Inventory.HasItem(Chapter01Weapon.ItemId)) return;
            if (_service.Inventory.TryGetEquipped(EquipmentSlot.Weapon, out _)) _service.Unequip(EquipmentSlot.Weapon);
            else _service.Equip(Chapter01Weapon.ItemId, EquipmentSlot.Weapon);
            Refresh();
        }
        private void InventoryChanged(InventoryChangedEvent value) => Refresh();
        private void Granted(EquipmentGrantedEvent value)
        {
            if (value.ItemId != Chapter01Weapon.ItemId) return;
            _toastText.text = "ПОЛУЧЕНО: ИЗЛУЧАТЕЛЬ М-0\nОбычный · ранг " + _service.Inventory.GetRank(value.ItemId) + "/5\nМеню → Снаряжение";
            _toastRemaining=4f;_toast.gameObject.SetActive(true);
        }
        private void Update()
        {
            if (_toastRemaining <= 0) return;
            _toastRemaining -= Time.unscaledDeltaTime;
            if (_toastRemaining <= 0) _toast.gameObject.SetActive(false);
        }
        private void Refresh()
        {
            var owned=_service.Inventory.HasItem(Chapter01Weapon.ItemId);
            var item=_service.Catalog.GetRequired(Chapter01Weapon.ItemId);
            var rank=_service.Inventory.GetRank(item.Id);
            var equipped=_service.Inventory.TryGetEquipped(EquipmentSlot.Weapon,out _);
            _model.sprite=_skin?.WeaponThumbnail(rank);_model.enabled=_model.sprite!=null;
            _model.color=owned?Color.white:new Color(.45f,.49f,.50f,1);
            if(_selectionFrame!=null)_selectionFrame.sprite=equipped?_skin.Selected:_skin.Frame;
            _selection.text=owned?(equipped?"● УСТАНОВЛЕНО · РАНГ "+rank:"ВЫБРАНО · В ИНВЕНТАРЕ"):"МОДЕЛЬ НАГРАДЫ · НЕ ПОЛУЧЕНО";
            _description.text=owned ? "Обычный · ранг "+rank+" / "+item.MaximumRank+"\n\nУрон +"+item.ModifierAtRank(rank).BaseDamage.ToString("0.#")+
                "\nПовторная победа повышает ранг до 5." :
                "Обычный · не получено\nПобедите Кустодиана М-0. Первая копия устанавливается на мех.";
            _description.resizeTextForBestFit=true;_description.resizeTextMinSize=18;_description.resizeTextMaxSize=24;
            _toggle.interactable=owned;_toggle.GetComponentInChildren<Text>().text=!owned?"НУЖНА ПОБЕДА НАД БОССОМ":equipped?"СНЯТЬ ОРУЖИЕ":"УСТАНОВИТЬ";
        }
        private void OnDestroy()
        { if(_service!=null){_service.InventoryChanged-=InventoryChanged;_service.EquipmentGranted-=Granted;} }
    }
}
