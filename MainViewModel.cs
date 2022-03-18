using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace Fnr
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Proc _proc;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand Check { get; }
        public ICommand Fnr { get; }
        public ICommand Cancel { get; }

        private string _file_mask = "*.*";
        public string File_mask
        {
            get => _file_mask;

            set
            {
                _file_mask = value;
                OnPropertyChanged(nameof(File_mask));
            }
        }

        private string _exclude_mask = "";
        public string Exclude_mask
        {
            get => _exclude_mask;

            set
            {
                _exclude_mask = value;
                OnPropertyChanged(nameof(Exclude_mask));
            }
        }

        private bool _all_dir = false;
        public bool All_dir
        {
            get => _all_dir;

            set
            {
                _all_dir = value;
                OnPropertyChanged(nameof(All_dir));
            }
        }

        private bool _block = false;
        public bool Block
        {
            get => _block;

            set
            {
                _block = value;
                OnPropertyChanged(nameof(Block));
            }
        }

        private string _dir = "";
        public string Dir
        {
            get => _dir;

            set
            {
                _dir = value;
                if (Directory.Exists(Dir)) Block = true;
                else Block = false;
                OnPropertyChanged(nameof(Dir));
            }
        }

        private string _find_block = "";
        public string Find_block
        {
            get => _find_block;

            set
            {
                _find_block = value;
                OnPropertyChanged(nameof(Find_block));
            }
        }
        private string _replace_block = "";
        public string Replace_block
        {
            get => _replace_block;

            set
            {
                _replace_block = value;
                OnPropertyChanged(nameof(Replace_block));
            }
        }

        public ObservableCollection<Results> Result { get; }
        public MainViewModel()
        {
            Result = new ObservableCollection<Results>();
            CancellationTokenSource Source = new CancellationTokenSource();
            CancellationToken token = Source.Token;

            Check = new RelayCommand<string>(x =>
            {
                if (!Directory.Exists(Dir))
                {
                    Dir = "Директория не существует!";
                }
            });

            Fnr = new RelayCommand<string>(x =>
            {
                Result.Clear();
                _proc = new Proc();
                _proc.Dir = Dir;
                _proc.File_mask = File_mask;
                _proc.Find_block = Find_block;
                _proc.Exclude_mask = Exclude_mask;
                _proc.All_dir = All_dir;
                if (x == "Find only")
                {
                    _proc.Is_replace = false;
                }
                else if (x == "Replace")
                {
                    _proc.Is_replace = true;
                    _proc.Replace_block = Replace_block;
                }
                Block = false;
                Task.Run(() => _proc.Get_files()).ContinueWith(res =>
                {
                    foreach (var file in _proc.FNR(res.Result))
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }
                        if (file != null)
                        {
                            App.Current.Dispatcher.Invoke((Action)delegate
                            {
                                Result.Add(file);
                            });
                        }
                    }
                });
                Block = true;
            });

            Cancel = new RelayCommand<string>(x =>
            {
                Source.Cancel();
            });
        }
    }
}
