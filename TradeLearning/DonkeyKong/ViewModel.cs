using DonkeyKong.Model;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DonkeyKong
{
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

        private int levelNumber;
        public int LevelNumber { get => levelNumber; set => SetProperty(ref levelNumber, value); }


        private Level editLevel;
        public Level EditLevel { get => editLevel; set => SetProperty(ref editLevel, value); }

        public IEnumerable<Level> Levels => Level.Levels;
    }
}