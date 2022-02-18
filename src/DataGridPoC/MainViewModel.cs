using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DataGridPoC.Annotations;

namespace DataGridPoC
{
    public class MainViewModel
    {
        public List<SomeRecord> RecordsItemsSource { get; set; }

        public MainViewModel()
        {
            RecordsItemsSource = Enumerable.Range(0, 500).Select(x => new SomeRecord(x)).ToList();
        }
    }

    public class SomeRecord : INotifyPropertyChanged
    {
        private List<SomeRecordNested> _nestedItemsSource = Enumerable.Range(0, 5).Select(x => new SomeRecordNested(x)).ToList();
        public Guid Id { get; set; } = Guid.NewGuid();

        public SomeRecord(int i)
        {
            IsOn = i % 2 == 0;

            NestedItemsSource = Enumerable.Range(0, 6).Select(x => new SomeRecordNested(x)).ToList();

            OnPropertyChanged(nameof(NestedItemsSource));
        }

        public bool IsOn { get; set; }

        public List<SomeRecordNested> NestedItemsSource
        {
            get => _nestedItemsSource;
            set
            {
                _nestedItemsSource = value;
                OnPropertyChanged(nameof(NestedItemsSource));
            }
        }

        public SomeRecordNested SelectedNested { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SomeRecordNested
    {
        public SomeRecordNested(int i)
        {
            Name = i.ToString();
        }

        public string Name { get; set; } = "hello world";
    }
}
