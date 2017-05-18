using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AcManager.Controls.Helpers;
using AcManager.Tools.Helpers;
using AcManager.Tools.Managers;
using AcManager.Tools.Miscellaneous;
using AcManager.Tools.Objects;
using AcTools.Render.Kn5SpecificForward;
using AcTools.Render.Kn5SpecificForwardDark;
using AcTools.Render.Kn5SpecificForwardDark.Lights;
using FirstFloor.ModernUI;
using FirstFloor.ModernUI.Commands;
using FirstFloor.ModernUI.Helpers;
using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Attached;
using JetBrains.Annotations;

namespace AcManager.Controls.CustomShowroom {
    public class CarSlotWrapped : NotifyPropertyChanged, IDraggable {
        [NotNull]
        public ForwardKn5ObjectRenderer.CarSlot Slot { get; }

        private CarObject _car;

        [CanBeNull]
        public CarObject Car {
            get { return _car; }
            set {
                if (Equals(value, _car)) return;
                _car = value;
                OnPropertyChanged();
            }
        }

        private CarSkinObject _skin;

        [CanBeNull]
        public CarSkinObject Skin {
            get { return _skin; }
            set {
                if (Equals(value, _skin)) return;
                _skin = value;
                OnPropertyChanged();
            }
        }

        public CarSlotWrapped(ForwardKn5ObjectRenderer.CarSlot slot) {
            Slot = slot;
            SyncCar();
        }

        private void SyncCar() {
            var node = Slot.CarNode;
            if (node == null) {
                Car = null;
                Skin = null;
            } else {
                Car = CarsManager.Instance.GetById(Path.GetDirectoryName(node.RootDirectory) ?? "");
                Skin = Car != null ? (Car.GetSkinById(node.CurrentSkin ?? "") ?? Car.SelectedSkin) : null;
            }
        }

        private bool _isDeleted;

        public bool IsDeleted {
            get { return _isDeleted; }
            set {
                if (Equals(value, _isDeleted)) return;
                _isDeleted = value;
                OnPropertyChanged();
            }
        }

        private DelegateCommand _deleteCommand;

        public DelegateCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new DelegateCommand(() => {
            IsDeleted = true;
        }));


        public const string DraggableFormat = "Data-CarSlotWrapped";

        string IDraggable.DraggableFormat => DraggableFormat;
    }

    public class DarkRendererCars : INotifyPropertyChanged, IUserPresetable {
        public static readonly string DefaultKey = "__DarkRendererSettings";
        public static readonly string DefaultPresetableKeyValue = "Custom Showroom (Cars)";

        static DarkRendererCars() {
            Draggable.RegisterDraggable<CarSlotWrapped>();
        }

        public DarkRendererCars(DarkKn5ObjectRenderer renderer) : this(renderer, DefaultPresetableKeyValue) {
            Initialize(false);
        }

        protected DarkRendererCars(DarkKn5ObjectRenderer renderer, string presetableKeyValue) {
            _presetableKeyValue = presetableKeyValue;
            Renderer = renderer;
        }

        protected virtual SaveableData CreateSaveableData() {
            return new SaveableData();
        }

        [NotNull]
        protected virtual ISaveHelper CreateSaveable() {
            return new SaveHelper<SaveableData>(DefaultKey, () => Save(CreateSaveableData()), Load, () => {
                Reset(false);
            });
        }

        [NotNull]
        protected SaveableData Save([NotNull] SaveableData obj) {
            return obj;
        }

        protected void Load(SaveableData o) {
        }

        protected virtual void Reset(bool saveLater) {
            Load(CreateSaveableData());

            if (saveLater) {
                SaveLater();
            }
        }

        internal bool HasSavedData {
            get {
                if (_saveable == null) {
                    _saveable = CreateSaveable();
                }

                return _saveable.HasSavedData;
            }
        }

        /// <summary>
        /// Don�t forget to call it in overrided versions.
        /// </summary>
        public void Initialize(bool reset) {
            // ReSharper disable once VirtualMemberCallInConstructor
            _saveable = CreateSaveable();
            if (reset) {
                _saveable.Reset();
            } else {
                _saveable.LoadOrReset();
            }

            Slots = new BetterObservableCollection<CarSlotWrapped>(Renderer.CarSlots.Select(x => new CarSlotWrapped(x)));
            Renderer.PropertyChanged += OnRendererPropertyChanged;
        }

        public BetterObservableCollection<CarSlotWrapped> Slots { get; private set; }

        private void UpdateSlots() {
            var newList = Renderer.CarSlots.Select(x => Slots.FirstOrDefault(y => y.Slot == x) ?? new CarSlotWrapped(x));
            Slots.ReplaceIfDifferBy(newList);
        }

        protected virtual void OnRendererPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Renderer.CarSlots):
                    ActionExtension.InvokeInMainThread(() => {
                        UpdateSlots();
                        SaveLater();
                    });
                    break;
            }
        }

        [NotNull]
        public DarkKn5ObjectRenderer Renderer { get; }

        protected class SaveableData {}

        [CanBeNull]
        private ISaveHelper _saveable;

        protected void SaveLater() {
            if (_saveable?.SaveLater() == true) {
                Changed?.Invoke(this, new EventArgs());
            }
        }

        #region Presets
        public bool CanBeSaved => true;

        private readonly string _presetableKeyValue;

        public string PresetableKey => _presetableKeyValue;

        string IUserPresetable.PresetableCategory => _presetableKeyValue;

        public string ExportToPresetData() {
            return _saveable?.ToSerializedString();
        }

        public event EventHandler Changed;

        public void ImportFromPresetData(string data) {
            _saveable?.FromSerializedString(data);
        }
        #endregion

        #region Share
        private ICommand _shareCommand;

        public ICommand ShareCommand => _shareCommand ?? (_shareCommand = new AsyncCommand(Share));

        protected virtual async Task Share() {
            var data = ExportToPresetData();
            if (data == null) return;
            await SharingUiHelper.ShareAsync(SharedEntryType.CustomShowroomPreset,
                    Path.GetFileNameWithoutExtension(UserPresetsControl.GetCurrentFilename(_presetableKeyValue)), null,
                    data);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null, bool save = true) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            if (save) {
                SaveLater();
            }
        }
    }
}