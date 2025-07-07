using DonkeyKong.Model;
using DonkeyKong.Model.Agents;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DonkeyKong
{
    public enum EngineState
    {
        Idle,
        Playing,
        Training,
        Editing
    }
    internal class ViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the property and raises the PropertyChanged event if the value has changed.
        /// </summary>
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private EngineState state;
        public EngineState State { get => state; set => SetProperty(ref state, value); }

        private int levelNumber;
        public int LevelNumber { get => levelNumber; set => SetProperty(ref levelNumber, value); }


        private Level editLevel;
        public Level EditLevel { get => editLevel; set => SetProperty(ref editLevel, value); }

        public IEnumerable<Level> Levels => Level.Levels;

        private IAgent agent;
        public IAgent Agent { get => agent; set => SetProperty(ref agent, value); }

        private List<IAgent> agents;
        public List<IAgent> Agents { get => agents; set => SetProperty(ref agents, value); }
    }
}