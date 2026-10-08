using Gravivore.Gameplay.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace Gravivore.Presentation.UI
{
    public sealed class WeaponEquipmentPanel : MonoBehaviour
    {
        private EquipmentService _service;
        private RectTransform _panel, _toast;
        private Text _description, _toastText;
        private Button _toggle;
        private float _toastRemaining;
        public bool IsVisible => _panel != null && _panel.gameObject.activeSelf;
        public string Description => _description.text;
        public void Initialize(RectTransform menuRoot, RectTransform hudRoot, EquipmentService service)
        {
            _service = service;
            var menu = (RectTransform)menuRoot.Find("Game Menu");
            menu.Find("Equipment Title").gameObject.SetActive(false);
            HudUiFactory.CreateButton(menu, "Open Equipment", new Vector2(.08f,.42f), new Vector2(.92f,.49f), "СНАРЯЖЕНИЕ", Open);
            _panel = HudUiFactory.CreatePanel(menuRoot, "Equipment Panel", new Vector2(.07f,.08f), new Vector2(.93f,.92f), HudUiFactory.PanelColor, true);
            HudUiFactory.CreateText(_panel, "Title", new Vector2(.07f,.87f),new Vector2(.93f,.97f),"ОРУЖИЕ",34,TextAnchor.MiddleCenter,Color.white);
            HudUiFactory.CreateText(_panel, "Item", new Vector2(.08f,.68f),new Vector2(.92f,.86f),"ИМПУЛЬСНЫЙ ИЗЛУЧАТЕЛЬ M-0",30,TextAnchor.MiddleCenter,HudUiFactory.AccentColor);
            _description = HudUiFactory.CreateText(_panel,"Description",new Vector2(.08f,.32f),new Vector2(.92f,.66f),"",26,TextAnchor.UpperLeft,Color.white);
            _toggle = HudUiFactory.CreateButton(_panel,"Equip Weapon",new Vector2(.08f,.18f),new Vector2(.92f,.28f),"",ToggleWeapon);
            HudUiFactory.CreateButton(_panel,"Back",new Vector2(.08f,.05f),new Vector2(.92f,.14f),"НАЗАД",Close);
            _panel.gameObject.SetActive(false);
            _toast = HudUiFactory.CreatePanel(hudRoot,"Weapon Loot Toast",new Vector2(.08f,.50f),new Vector2(.92f,.62f),HudUiFactory.PanelColor);
            _toastText = HudUiFactory.CreateText(_toast,"Loot",new Vector2(.04f,.06f),new Vector2(.96f,.94f),"",26,TextAnchor.MiddleCenter,HudUiFactory.AccentColor);
            _toast.gameObject.AddComponent<CanvasGroup>().blocksRaycasts=false; _toast.gameObject.SetActive(false);
            service.InventoryChanged += InventoryChanged; service.EquipmentGranted += Granted;
            Refresh();
        }
        public void Open() { Refresh(); _panel.SetAsLastSibling(); _panel.gameObject.SetActive(true); }
        public void Close() { if (_panel != null) _panel.gameObject.SetActive(false); }
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
            _toastText.text = "ПОЛУЧЕНО: ИЗЛУЧАТЕЛЬ M-0\nОбычный · ранг " + _service.Inventory.GetRank(value.ItemId) + "/5\nМеню → Снаряжение";
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
            _description.text=owned ? "Обычный · ранг "+rank+" / "+item.MaximumRank+"\n\nУрон +"+item.ModifierAtRank(rank).BaseDamage.ToString("0.#")+
                "\n"+(equipped?"УСТАНОВЛЕНО НА ПРАВЫЙ МАНИПУЛЯТОР":"В ИНВЕНТАРЕ")+"\n\nПовторная победа над Кустодианом повышает ранг до 5." :
                "Обычный · не получено\n\nПобедите Кустодиана M-0, чтобы получить оружие.\n\nПервая копия устанавливается на G-0. Повторные копии повышают ранг до 5.";
            _toggle.interactable=owned;_toggle.GetComponentInChildren<Text>().text=!owned?"НУЖНА ПОБЕДА НАД БОССОМ":equipped?"СНЯТЬ ОРУЖИЕ":"УСТАНОВИТЬ";
        }
        private void OnDestroy()
        { if(_service!=null){_service.InventoryChanged-=InventoryChanged;_service.EquipmentGranted-=Granted;} }
    }
}
