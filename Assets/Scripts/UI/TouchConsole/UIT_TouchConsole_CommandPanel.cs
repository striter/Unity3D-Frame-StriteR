using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TDataPersistent;
using UnityEngine;
using UnityEngine.UI;
using static UIT_TouchConsole;
public static class UIT_TouchConsoleHelper
{
    public static string GetKeyCodeString(this KeyCode _keyCode) => _keyCode == KeyCode.None ? "" : _keyCode.ToString();
    public static void Button(this CommandContainer _container, Action OnClick)
    {
        CommandItem_Button button = _container.Insert<CommandItem_Button>();
        button.m_Button.onClick.AddListener(() => OnClick());
        button.m_ButtonTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Toggle(this CommandContainer _container, Ref<bool> _refValue, Action<bool> OnToggleChange)
    {
        CommandItem_Toggle toggle = _container.Insert<CommandItem_Toggle>();
        toggle.SetDataUpdate(() => toggle.m_Toggle.isOn = _refValue.m_RefValue);
        toggle.m_Toggle.onValueChanged.AddListener(value => {
            _refValue.SetValue(value);
            OnToggleChange(value);
        });
        toggle.m_ToggleTitle.text = _container.m_KeyCode.GetKeyCodeString();
    }
    public static void Slider(this CommandContainer _container, int _minValue, int _maxValue, Ref<int> _refValue, Action<int> _SetValue, string _format = "{0}") => Slider(_container, _minValue, _maxValue, _refValue.m_RefValue, value => { _refValue.SetValue((int)value); _SetValue(_refValue.m_RefValue); }, _format, true);
    public static void Slider(this CommandContainer _container, float _minValue, float _maxValue, Ref<float> _refValue, Action<float> _SetValue, string _format = "{0:0.0}", bool _wholeNumbers = false)
    {
        CommandItem_Slider slider = _container.Insert<CommandItem_Slider>();
        slider.m_Slider.wholeNumbers = _wholeNumbers;
        slider.m_Slider.minValue = _minValue;
        slider.m_Slider.maxValue = _maxValue;
        slider.SetDataUpdate(() => {
            float finalValue = _refValue.m_RefValue;
            slider.m_Slider.value = finalValue;
            slider.m_Value.text = string.Format(_format, finalValue);
        });
        slider.m_Slider.onValueChanged.AddListener(value => {
            slider.m_Value.text = string.Format(_format, value);
            _refValue.SetValue(value);
            _SetValue(value);
        });
    }
    static T ButtonFoldOutItem<T>(this CommandContainer _container, bool foldOut, out CommandItem_Button _button) where T : CommandItemBase
    {
        _button = null;
        if (!foldOut)
            return _container.Insert<T>();
        _button = _container.Insert<CommandItem_Button>();
        T item = _container.Insert<T>();
        _button.m_Button.onClick.AddListener(() => item.transform.SetActive(!item.transform.gameObject.activeSelf));
        item.transform.SetActive(false);
        return item;
    }
    public static void EnumSelection<T>(this CommandContainer _container, Ref<T> _valueRef, Action<T> OnClick, bool foldOut = true) where T : struct, Enum => EnumSelection(_container, _valueRef.m_RefValue, enumObj => { _valueRef.SetValue((T)enumObj); OnClick?.Invoke(_valueRef.m_RefValue); }, foldOut);
    public static void EnumSelection(this CommandContainer _container, object _value, Action<object> OnClick, bool foldOut = true)
    {
        Type type = _value.GetType();
        if (!type.IsEnum)
            throw new Exception("Input Must Be Enum!");
        IEnumerable<object> enumNumerable = Enum.GetValues(type).GetEnumerable();
        EnumSelection(_container, enumNumerable.FindIndex(p=>p.Equals(_value)), enumNumerable.ToList(obj=>obj.ToString()),value=>OnClick(Enum.ToObject(type,value)),foldOut);
    }
    public static void EnumSelection(this CommandContainer _container, Ref<int> _refEnum, List<string> _values, Action<int> OnClick, bool foldOut = true)
    {
        CommandItem_ButtonSelection selection = _container.ButtonFoldOutItem<CommandItem_ButtonSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => {
            selection.Highlight(_refEnum.m_RefValue);
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = _values[_refEnum.m_RefValue];
        });
        selection.Play(_values, (int value) => {
            OnClick(value);
            foreach (var button in selection.m_ButtonGrid.m_ActiveItems.Values)
                button.Highlight(button.m_Identity == value);
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = _values[value];
        }).Highlight(_refEnum.m_RefValue);
    }
    public static void FlagsSelection<T>(this CommandContainer _container, Ref<T> _refFlags, Action<T> _logFilter, bool foldOut = true) where T : struct, Enum
    {
        CommandItem_FlagsSelection selection = _container.ButtonFoldOutItem<CommandItem_FlagsSelection>(foldOut, out CommandItem_Button foldOutButton);
        selection.SetDataUpdate(() => selection.Play(_refFlags.m_RefValue, flags => {
            _refFlags.SetValue(flags);
            if (foldOutButton != null)
                foldOutButton.m_ButtonTitle.text = flags.GetNumerable().ToString('|', value => value ? "��" : "��");
            _logFilter(flags);
        }));
        if (foldOutButton != null)
            foldOutButton.m_ButtonTitle.text = _refFlags.m_RefValue.GetNumerable().ToString('|', value => value ? "��" : "��");
    }
    public static void InputField(this CommandContainer _container, Ref<string> _refText, Action<string> OnValueClick)
    {
        CommandItem_InputField input = _container.Insert<CommandItem_InputField>();
        input.SetDataUpdate(() => { input.m_InputField.text = _refText.m_RefValue; });
        input.m_InputField.onValueChanged.AddListener(_refText.SetValue);
        _container.Button(() => OnValueClick(input.m_InputField.text));
    }
    public static void InpuptField(this CommandContainer _container, Ref<string> _refText1, Ref<string> _refText2, Action<string, string> OnValueClick)
    {
        CommandItem_InputField input1 = _container.Insert<CommandItem_InputField>();
        CommandItem_InputField input2 = _container.Insert<CommandItem_InputField>();
        input1.SetDataUpdate(() => input1.m_InputField.text = _refText1.m_RefValue);
        input2.SetDataUpdate(() => input2.m_InputField.text = _refText2.m_RefValue);
        input1.m_InputField.onValueChanged.AddListener(_refText1.SetValue);
        input2.m_InputField.onValueChanged.AddListener(_refText2.SetValue);
        _container.Button(() => OnValueClick(input1.m_InputField.text, input2.m_InputField.text));
    }



}
public partial class UIT_TouchConsole : SingletonMono<UIT_TouchConsole>,IPartialMethods<enum_PartialMethods,enum_PartialSorting>
{
    public static void NewPage(string _page)
    {
        Instance.AddNewPage(_page);
        EmptyLine();
        Header(_page);
        Instance.SelectPage(Instance.m_PageSelection.Count-1);
    }
    public static void EmptyLine() => Instance.AddCommandLine();
    public static void Header(string _title) => Instance.AddCommandLine().Insert<CommandItem_Header>().m_HeaderTitle.text = _title;
    public static CommandContainer Command(string _title, KeyCode _keyCode = KeyCode.None)
    {
        CommandContainer container = Instance.AddCommandLine(_keyCode);
        container.Insert<CommandItem_CommandTitle>().m_CommandTitle.text = _title;
        return container;
    }
    public static void InitSerializeCommands<T>(T _target, Action<T> _OnSerializeDataChanged) where T : MonoBehaviour {
        Type targetType = _target.GetType();
        Command("Enable").Toggle(_target.enabled, value => _target.enabled = value);
        foreach (var fieldStack in targetType.GetBaseTypeFieldStacks(BindingFlags.Instance))
        {
            object startValue = fieldStack.Value.GetValue(_target);
            if (fieldStack.Key.FieldType.IsEnum)
            {
                Command(fieldStack.Key.Name).EnumSelection(startValue, value => fieldStack.Value.SetValue(_target, value));
                continue;
            }
            if (fieldStack.Key.FieldType == typeof(bool))
            {
                Command(fieldStack.Key.Name).Toggle((bool)startValue, value => fieldStack.Value.SetValue(_target, value));
                continue;
            }
            if (fieldStack.Key.FieldType == typeof(string))
            {
                Command(fieldStack.Key.Name).InputField((string)startValue, value => fieldStack.Value.SetValue(_target, value));
                continue;
            }

            var attributes = fieldStack.Key.GetCustomAttributes(false);
            foreach (var attribute in attributes)
            {
                if (attribute is RangeAttribute)
                {
                    RangeAttribute rangeAttribute = attribute as RangeAttribute;
                    if (fieldStack.Key.FieldType == typeof(int))
                        Command(fieldStack.Key.Name).Slider((int)rangeAttribute.min, (int)rangeAttribute.max, (int)startValue, value => { fieldStack.Value.SetValue(_target, value); _OnSerializeDataChanged?.Invoke(_target); });
                    else
                        Command(fieldStack.Key.Name).Slider(rangeAttribute.min, rangeAttribute.max, (float)startValue, value => { fieldStack.Value.SetValue(_target, value); _OnSerializeDataChanged?.Invoke(_target); });
                    continue;
                }
                else if (attribute is RangeVectorAttribute)
                {
                    RangeVectorAttribute vectorAttribute = attribute as RangeVectorAttribute;
                    CommandContainer command = Command(fieldStack.Key.Name);
                    Type fieldType = fieldStack.Key.FieldType;
                    if (fieldType == typeof(Vector2))
                    {
                        Vector2 startVec = (Vector2)startValue;
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.x, value => { fieldStack.Value.SetValue(_target, ((Vector2)fieldStack.Value.GetValue(_target)).SetX(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.y, value => { fieldStack.Value.SetValue(_target, ((Vector2)fieldStack.Value.GetValue(_target)).SetY(value)); _OnSerializeDataChanged?.Invoke(_target); }, "Y:{0:0.0}");
                    }
                    else if (fieldType == typeof(Vector3))
                    {
                        Vector3 startVec = (Vector3)startValue;
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.x, value => { fieldStack.Value.SetValue(_target, ((Vector3)fieldStack.Value.GetValue(_target)).SetX(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.y, value => { fieldStack.Value.SetValue(_target, ((Vector3)fieldStack.Value.GetValue(_target)).SetY(value)); _OnSerializeDataChanged?.Invoke(_target); }, "Y:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.z, value => { fieldStack.Value.SetValue(_target, ((Vector3)fieldStack.Value.GetValue(_target)).SetZ(value)); _OnSerializeDataChanged?.Invoke(_target); }, "Z:{0:0.0}");
                    }
                    else if (fieldType == typeof(Vector4))
                    {
                        Vector4 startVec = (Vector4)startValue;
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.x, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetX(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.y, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetY(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.z, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetZ(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                        command.Slider(vectorAttribute.m_Min, vectorAttribute.m_Max, startVec.w, value => { fieldStack.Value.SetValue(_target, ((Vector4)fieldStack.Value.GetValue(_target)).SetW(value)); _OnSerializeDataChanged?.Invoke(_target); }, "X:{0:0.0}");
                    }
                }
            }
        }
    }

    Timer m_FastKeyCooldownTimer = new Timer(.5f);
    public bool m_ConsoleOpening { get; private set; } = false;
    ScrollRect m_ConsoleCommandScrollRect;
    TGameObjectPool_Instance_Class<int, CommandContainer> m_CommandContainers;
    int m_CurrentPage;
    TGameObjectPool_Instance_Class<int, ButtonSelect> m_PageSelection;
    Dictionary<Type, TGameObjectPool_Instance_Class<int, CommandItemBase>> m_CommandItems = new Dictionary<Type, TGameObjectPool_Instance_Class<int, CommandItemBase>>();

    Action<bool> OnConsoleShow;
    public void SetOnConsoleShow(Action<bool> _OnConsoleShow)
    {
        OnConsoleShow = _OnConsoleShow;
    }
    [PartialMethod(enum_PartialMethods.Init,enum_PartialSorting.CommandConsole)]
    void InitConsole()
    {
        m_ConsoleCommandScrollRect = transform.Find("Command").GetComponent<ScrollRect>();
        m_CommandContainers = new TGameObjectPool_Instance_Class<int, CommandContainer>(m_ConsoleCommandScrollRect.transform.Find("Viewport/Content"), "GridItem");
        Transform containerItemPool = m_ConsoleCommandScrollRect.transform.Find("Viewport/CommandItemPool");
        UReflection.TraversalAllInheritedClasses<CommandItemBase>(type => m_CommandItems.Add(type, new TGameObjectPool_Instance_Class<int, CommandItemBase>(containerItemPool, type, type.Name)));

        m_ConsoleOpening = false;
        m_ConsoleCommandScrollRect.SetActive(m_ConsoleOpening);

        m_PageSelection = new TGameObjectPool_Instance_Class<int, ButtonSelect>(m_ConsoleCommandScrollRect.transform.Find("Viewport/Content/PageSelect"),"GridItem");
    }
    [PartialMethod(enum_PartialMethods.Reset,enum_PartialSorting.CommandConsole)]
    void ResetConsole()
    {
        m_PageSelection.Clear();
        m_CurrentPage = -1;
        m_CommandContainers.Clear();
    }
    [PartialMethod(enum_PartialMethods.Tick,enum_PartialSorting.CommandConsole)]
    internal void TickConsole(float _deltaTime)
    {
        m_CommandContainers.m_ActiveItems.Traversal(command => command.KeycodeTick());

        m_FastKeyCooldownTimer.Tick(_deltaTime);
        if (m_FastKeyCooldownTimer.m_Timing)
            return;
        if (Input.touchCount >= 5 || Input.GetKey(KeyCode.BackQuote))
        {
            m_FastKeyCooldownTimer.Replay();
            m_ConsoleOpening = !m_ConsoleOpening;
            m_ConsoleCommandScrollRect.SetActive(m_ConsoleOpening);
            OnConsoleShow?.Invoke(m_ConsoleOpening);

            Time.timeScale = m_ConsoleOpening ? m_Data.m_ConsoleTimeScale.m_RefValue : 1f;
            UpdateLogs();
            UpdateCommandData();
        }
    }
    void AddNewPage(string _page) => m_PageSelection.AddItem(m_PageSelection.Count).Init(_page, SelectPage);
    void UpdateCommandData() => m_CommandContainers.m_ActiveItems.Traversal(command => command.UpdateItems());
    CommandContainer AddCommandLine(KeyCode _keyCode = KeyCode.None) => m_CommandContainers.AddItem(m_CommandContainers.Count).Init(m_PageSelection.Count - 1, _keyCode, CommandItemCreate, CommandItemRecycle);
    CommandItemBase CommandItemCreate(Type type) => m_CommandItems[type].AddItem(m_CommandItems[type].Count);
    void CommandItemRecycle(CommandItemBase item) => m_CommandItems[item.GetType()].RemoveItem(item.m_Identity);
    void SelectPage(int _page)
    {
        m_CurrentPage = _page;
        foreach (var page in m_PageSelection.m_ActiveItems.Values)
            page.Highlight(page.m_Identity == m_CurrentPage);
        foreach (var command in m_CommandContainers.m_ActiveItems.Values)
            command.transform.SetActive(command.m_PageIndex == m_CurrentPage);
        LayoutRebuilder.ForceRebuildLayoutImmediate(m_LogPanelRect.transform as RectTransform);
    }
    void SetConsoleTimeScale(float _timeScale)
    {
        if (!m_ConsoleOpening)
            return;
        Time.timeScale = _timeScale;
    }
    public class ButtonSelect : CGameObjectPool_Instance_Class<int>
    {
        Text m_Title;
        Transform m_Highlight;
        Action<int> OnClick;
        public ButtonSelect(Transform _transform) : base(_transform)
        {
            transform.GetComponentInChildren<Button>().onClick.AddListener(() => OnClick?.Invoke(m_Identity));
            m_Title = transform.GetComponentInChildren<Text>();
            m_Highlight = transform.Find("Highlight");
        }
        public void Init(string _title, Action<int> _OnClick)
        {
            m_Title.text = _title;
            OnClick = _OnClick;
            Highlight(false);
        }
        public void Highlight(bool _highlight)
        {
            m_Highlight.SetActive(_highlight);
        }
    }
    public class CommandContainer : CGameObjectPool_Instance_Class<int>
    {
        #region Predefine Classes
        #endregion
        List<CommandItemBase> m_Items = new List<CommandItemBase>();
        public CommandContainer(Transform _transform) : base(_transform) { }
        public int m_PageIndex { get; private set; }
        public KeyCode m_KeyCode { get; private set; }
        Func<Type, CommandItemBase> CreateItem;
        Action<CommandItemBase> RecycleItem;
        public CommandContainer Init(int _pageIndex, KeyCode _keyCode, Func<Type, CommandItemBase> _CreateItem, Action<CommandItemBase> _RecycleItem)
        {
            m_PageIndex = _pageIndex;
            m_KeyCode = _keyCode;
            CreateItem = _CreateItem;
            RecycleItem = _RecycleItem;
            return this;
        }
        public void KeycodeTick()
        {
            if (m_KeyCode == KeyCode.None)
                return;

            if (Input.GetKeyDown(m_KeyCode))
                m_Items.Traversal(item => item.OnFastKeyTrigger());
        }
        public void UpdateItems() => m_Items.Traversal(item => item.OnDataUpdated?.Invoke());
        public T Insert<T>() where T : CommandItemBase
        {
            T item = CreateItem(typeof(T)) as T;
            item.transform.SetParent(transform);
            m_Items.Add(item);
            return item;
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Items.Traversal(RecycleItem);
            m_Items.Clear();
            m_KeyCode = KeyCode.None;
        }
    }
    public class CommandItemBase : CGameObjectPool_Instance_Class<int>
    {
        public CommandItemBase(Transform _transform) : base(_transform) { }
        public virtual void OnFastKeyTrigger() { }
        public Action OnDataUpdated { get; private set; }
        public void SetDataUpdate(Action _OnDataUpdated)
        {
            OnDataUpdated = _OnDataUpdated;
            OnDataUpdated?.Invoke();
        }
    }
    public class CommandItem_FlagsSelection : CommandItemBase
    {
        TGameObjectPool_Component<int, Toggle> m_ToggleGrid;
        public CommandItem_FlagsSelection(Transform _transform) : base(_transform)
        {
            m_ToggleGrid = new TGameObjectPool_Component<int, Toggle>(_transform, "GridItem");
        }
        public void Play<T>(T defaultValue, Action<T> _OnFlagChanged) where T : Enum
        {
            m_ToggleGrid.Clear();
            foreach (T enumValue in UCommon.GetEnumValues<T>())
            {
                Toggle tog = m_ToggleGrid.AddItem(Convert.ToInt32(enumValue));
                tog.isOn = defaultValue.IsFlagEnable(enumValue);
                tog.GetComponentInChildren<Text>().text = enumValue.ToString();
                tog.onValueChanged.RemoveAllListeners();
                tog.onValueChanged.AddListener(changed => {
                    int totalIndex = 0;
                    m_ToggleGrid.m_ActiveItems.Traversal((index, toggle) => totalIndex += (toggle.isOn ? index : 0));
                    _OnFlagChanged((T)Enum.ToObject(typeof(T), totalIndex));
                });
            }
        }
    }
    public class CommandItem_ButtonSelection : CommandItemBase
    {
        public TGameObjectPool_Instance_Class<int, ButtonSelect> m_ButtonGrid { get; private set; }
        public CommandItem_ButtonSelection(Transform _transform) : base(_transform)
        {
            m_ButtonGrid = new TGameObjectPool_Instance_Class<int, ButtonSelect>(_transform, "GridItem");
        }
        public CommandItem_ButtonSelection Play(List<string> values, Action<int> _OnClick)
        {
            m_ButtonGrid.Clear();
            values.Traversal((int index, string temp) =>
            {
                ButtonSelect btn = m_ButtonGrid.AddItem(index);
                btn.Init(temp, _OnClick);
            });
            return this;
        }
        public void Highlight(int _value)
        {
            foreach (var item in m_ButtonGrid.m_ActiveItems.Values)
                item.Highlight(item.m_Identity == _value);
        }
    }
    public class CommandItem_Header : CommandItemBase
    {
        public Text m_HeaderTitle { get; private set; }
        public CommandItem_Header(Transform _transform) : base(_transform)
        {
            m_HeaderTitle = _transform.Find("Title").GetComponent<Text>();
        }
    }
    public class CommandItem_CommandTitle : CommandItemBase
    {
        public Text m_CommandTitle { get; private set; }
        public CommandItem_CommandTitle(Transform _transform) : base(_transform)
        {
            m_CommandTitle = _transform.Find("Title").GetComponent<Text>();
        }
    }
    public class CommandItem_Toggle : CommandItemBase
    {
        public Toggle m_Toggle { get; private set; }
        public Text m_ToggleTitle { get; private set; }
        public CommandItem_Toggle(Transform _transform) : base(_transform)
        {
            m_Toggle = transform.GetComponent<Toggle>();
            m_ToggleTitle = _transform.Find("Title").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_Toggle.onValueChanged.RemoveAllListeners();
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Toggle.onValueChanged.RemoveAllListeners();
        }
        public override void OnFastKeyTrigger()
        {
            base.OnFastKeyTrigger();
            m_Toggle.isOn = !m_Toggle.isOn;
            m_Toggle.onValueChanged.Invoke(m_Toggle.isOn);
        }
    }
    public class CommandItem_Slider : CommandItemBase
    {
        public Slider m_Slider { get; private set; }
        public Text m_Value { get; private set; }
        public CommandItem_Slider(Transform _transform) : base(_transform)
        {
            m_Slider = transform.Find("Slider").GetComponent<Slider>();
            m_Value = transform.Find("Value").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_Slider.onValueChanged.RemoveAllListeners();
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Slider.onValueChanged.RemoveAllListeners();
        }
    }
    public class CommandItem_Button : CommandItemBase
    {
        public Button m_Button { get; private set; }
        public Text m_ButtonTitle { get; private set; }
        public CommandItem_Button(Transform _transform) : base(_transform)
        {
            m_Button = _transform.GetComponent<Button>();
            m_ButtonTitle = _transform.Find("Title").GetComponent<Text>();
        }
        public override void OnAddItem(int identity)
        {
            base.OnAddItem(identity);
            m_Button.onClick.RemoveAllListeners();
            m_ButtonTitle.text = "";
        }
        public override void OnRemoveItem()
        {
            base.OnRemoveItem();
            m_Button.onClick.RemoveAllListeners();
        }
        public override void OnFastKeyTrigger()
        {
            base.OnFastKeyTrigger();
            m_Button.onClick.Invoke();
        }
    }

    public class CommandItem_InputField : CommandItemBase
    {
        public InputField m_InputField { get; private set; }
        public CommandItem_InputField(Transform _transform) : base(_transform)
        {
            m_InputField = _transform.GetComponent<InputField>();
        }
        public override void OnInitItem(Action<int> DoRecycle)
        {
            base.OnInitItem(DoRecycle);
            m_InputField.onValueChanged.RemoveAllListeners();
        }

    }
}