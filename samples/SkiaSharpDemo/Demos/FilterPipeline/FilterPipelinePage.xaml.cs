using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Extended.UI.Media;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpDemo.Demos
{
	public partial class FilterPipelinePage : ContentPage
	{
		private bool enableFilters = true;
		private SKFilter? selectedFilter;

		public FilterPipelinePage()
		{
			InitializeComponent();

			Image = new SKImageImageSource
			{
				Image = SKImage.FromEncodedData(App.GetImageResourceStream("img1.jpg")).ToRasterImage(true)
			};

			ToggleAllFiltersCommand = new Command(OnToggleAllFilters);
			ToggleFilterCommand = new Command<SKFilter?>(OnToggleFilter);
			RemoveFilterCommand = new Command<SKFilter?>(OnRemoveFilter);
			AddNewFilterCommand = new Command(OnAddNewFilter);

			MoveUpCommand = new Command(OnMoveUp,
				() => SelectedFilter != null && Filters.IndexOf(SelectedFilter) > 0);
			MoveDownCommand = new Command(OnMoveDown,
				() => SelectedFilter != null && Filters.IndexOf(SelectedFilter) < Filters.Count - 1);

			Filters.CollectionChanged += OnFiltersCollectionChanged;

			BindingContext = this;
		}

		public ImageSource Image { get; }

		public ICommand ToggleAllFiltersCommand { get; }

		public ICommand ToggleFilterCommand { get; }

		public ICommand RemoveFilterCommand { get; }

		public Command AddNewFilterCommand { get; }

		public Command MoveUpCommand { get; }

		public Command MoveDownCommand { get; }

		public SKFilterCollection Filters { get; } = new SKFilterCollection
		{
			new SKBlurFilter { SigmaX = 5, SigmaY = 5 },
			new SKHighContrastFilter { Factor = 0.25 },
		};

		public bool EnableFilters
		{
			get => enableFilters;
			set
			{
				enableFilters = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<SKFilter> SelectedFilters { get; } = new ObservableCollection<SKFilter>();

		public SKFilter? SelectedFilter
		{
			get => selectedFilter;
			set
			{
				selectedFilter = value;
				OnPropertyChanged();

				if (selectedFilter == null)
					SelectedFilters.Clear();
				else if (SelectedFilters.Count == 0)
					SelectedFilters.Add(selectedFilter);
				else
					SelectedFilters[0] = selectedFilter;

				MoveUpCommand.ChangeCanExecute();
				MoveDownCommand.ChangeCanExecute();
			}
		}

		private void OnToggleAllFilters()
		{
			EnableFilters = !EnableFilters;
		}

		private void OnToggleFilter(SKFilter? filter)
		{
			if (filter == null)
				return;

			filter.IsEnabled = !filter.IsEnabled;
		}

		private void OnRemoveFilter(SKFilter? filter)
		{
			if (filter == null)
				return;

			if (SelectedFilter == filter)
				SelectedFilter = null;

			Filters.Remove(filter);
		}

		private void OnAddNewFilter()
		{
			var page = new NewFilterPage();
			page.PickedCommand = new Command<Type>(async type =>
			{
				var filter = (SKFilter)Activator.CreateInstance(type);
				Filters.Add(filter);
				SelectedFilter = filter;

				if (Device.RuntimePlatform == Device.UWP)
					await Task.Delay(100);

				await page.Navigation.PopAsync();
			});
			Navigation.PushAsync(page);
		}

		private void OnMoveUp()
		{
			if (SelectedFilter == null)
				return;

			var idx = Filters.IndexOf(SelectedFilter);
			if (idx == -1 || idx <= 0)
				return;

			Filters.Move(idx, idx - 1);
		}

		private void OnMoveDown()
		{
			if (SelectedFilter == null)
				return;

			var idx = Filters.IndexOf(SelectedFilter);
			if (idx == -1 || idx >= Filters.Count - 1)
				return;

			Filters.Move(idx, idx + 1);
		}

		private void OnFiltersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			MoveUpCommand.ChangeCanExecute();
			MoveDownCommand.ChangeCanExecute();
		}
	}
}
