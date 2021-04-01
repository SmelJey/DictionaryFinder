using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DictionaryFinder.Annotations;


namespace DictionaryFinder {
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const int DefaultItemsPerPage = 100;
        private int myItemsPerPage = DefaultItemsPerPage;

        private int myTotalResultsCnt;

        private ScrollViewer myListScrollViewer;

        private readonly WordDictionary myDictionary;
        private readonly Queue<string> myResponseBuffer = new Queue<string>();

        private CancellationTokenSource myLoadingTokenSource;
        private CancellationTokenSource myScrollHandlingTokenSource;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string SearchHeader => $"Search results ({SearchResults.Count} out of {myTotalResultsCnt}):";

        public int TotalResults
        {
            get => myTotalResultsCnt;
            set
            {
                myTotalResultsCnt = value;
                OnPropertyChanged(nameof(SearchHeader));
            }
        }

        public ObservableCollection<string> SearchResults { get; set; }

        public MainWindow() {
            myDictionary = new WordDictionary("dict.txt");
            SearchResults = new ObservableCollection<string>();
            InitializeComponent();

            Dispatcher.Invoke(async () =>
            {
                // idk when ListView's ScrollViewer is initialized
                while (myListScrollViewer == null)
                {
                    await Task.Delay(500);

                    myListScrollViewer = ResultView.GetChildOfType<ScrollViewer>();
                    if (myListScrollViewer != null) {
                        myListScrollViewer.ScrollChanged += ProcessScrolling;
                        return;
                    }
                }
            });
        }

        private void SearchField_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            StartSearchTask();
        }

        private void SeqSearchCheckbox_OnChecked(object sender, RoutedEventArgs e) {
            StartSearchTask();
        }

        private async void StartSearchTask()
        {
            myLoadingTokenSource?.Cancel();
            myLoadingTokenSource = new CancellationTokenSource();
            SearchResults.Clear();
            myResponseBuffer.Clear();
            TotalResults = 0;

            string request = SearchField.Text;
            if (request.Length == 0)
            {
                StatusLabel.Text = "Ready!";
                return;
            }

            StatusLabel.Text = "Search in progress...";
            bool isSeqSearch = SeqSearchCheckbox.IsChecked.GetValueOrDefault(false);

            await Task.Run(() => LoadData(request, isSeqSearch, myLoadingTokenSource.Token), myLoadingTokenSource.Token);
        }

        private Task LoadData(string request, bool isSeqSearch, CancellationToken ct)
        {
            foreach (var res in myDictionary.GetMatches(request, isSeqSearch)) {
                if (ct.IsCancellationRequested) {
                    return Task.CompletedTask;
                }

                TotalResults++;

                Dispatcher.Invoke(() =>
                {
                    if (ct.IsCancellationRequested) {
                        return;
                    }

                    if (SearchResults.Count >= myItemsPerPage)
                    {
                        myResponseBuffer.Enqueue(res);
                    }
                    else
                    {
                        AddItemToList(res);
                    }
                });
            
            }

            Dispatcher.Invoke(() => {
                StatusLabel.Text = "Ready!";
            }, DispatcherPriority.Background);

            return Task.CompletedTask;
        }

        private void ProcessScrolling(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange <= 0 || Math.Abs(e.VerticalChange - e.ExtentHeightChange) < float.Epsilon) {
                return;
            }

            if (Math.Abs(e.VerticalOffset + e.ViewportHeight - e.ExtentHeight) < float.Epsilon) {
                LoadBufferedData(myItemsPerPage);
                AfterScrollingLoad();
            }
        }

        private void AddItemToList(string item)
        {
            SearchResults.Add(item);
            OnPropertyChanged(nameof(SearchHeader));
        }

        private async void AfterScrollingLoad()
        {
            myScrollHandlingTokenSource?.Cancel();
            myScrollHandlingTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(1000, myScrollHandlingTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (Math.Abs(myListScrollViewer.VerticalOffset + myListScrollViewer.ViewportHeight - myListScrollViewer.ExtentHeight)
                < float.Epsilon)
            {
                LoadBufferedData(myItemsPerPage);
            }
        }

        private void LoadBufferedData(int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                if (myResponseBuffer.Count == 0)
                {
                    return;
                }

                AddItemToList(myResponseBuffer.Dequeue());
            }
        }

        private void ResultView_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            myItemsPerPage = (int)(e.NewSize.Height * 3 / 20);
            if (myItemsPerPage == 0)
            {
                myItemsPerPage = DefaultItemsPerPage;
            }

            if (SearchResults.Count < myItemsPerPage)
            {
                LoadBufferedData(myItemsPerPage - SearchResults.Count);
            }
        }
    }
}
