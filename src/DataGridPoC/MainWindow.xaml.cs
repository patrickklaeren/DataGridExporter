using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DataGridPoC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel();
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DataGridExporter.Export(DataGridToExport);
        }
    }

    public class ExportedCell
    {
        public ExportedCell(string value)
        {
            RawValue = value;
        }

        public string RawValue { get; set; }
    }

    public class ExportedBooleanCell : ExportedCell
    {
        public bool Value { get; set; }

        public ExportedBooleanCell(string value) : base(value)
        {
            Value = bool.Parse(value);
        }
    }

    public class DataGridExporter
    {
        public static void Export(DataGrid dataGridToExport)
        {
            var exportedCells = new List<ExportedCell>();

            var collectionView = CollectionViewSource.GetDefaultView(dataGridToExport.ItemsSource);

            foreach (var row in collectionView)
            {
                foreach (var column in dataGridToExport.Columns.OrderBy(a => a.DisplayIndex))
                {
                    ExportedCell? cell = null;

                    var exportString = ExportBehaviour.GetDataGridExport(column);

                    switch (column)
                    {
                        case DataGridTextColumn dataGridColumn when dataGridColumn.Binding is Binding cellBinding:
                            cell = ProcessTextColumn(row, cellBinding);
                            break;
                        case DataGridCheckBoxColumn dataGridColumn when dataGridColumn.Binding is Binding cellBinding:
                            cell = ProcessCheckboxColumn(row, cellBinding);
                            break;
                        case DataGridComboBoxColumn dataGridColumn when dataGridColumn.SelectedItemBinding is Binding cellBinding:
                            cell = ProcessComboboxColumn(row, cellBinding, dataGridColumn.DisplayMemberPath);
                            break;
                    }

                    if (cell is not null)
                    {
                        exportedCells.Add(cell);
                    }
                }
            }
        }

        private static ExportedCell? ProcessTextColumn(object item, Binding binding)
        {
            var boundProperty = binding.Path.Path;

            var propertyInfo = item.GetType().GetProperty(boundProperty);
            var propertyValue = propertyInfo?.GetValue(item);

            if (propertyValue is null)
            {
                return null;
            }

            return new ExportedCell(propertyValue.ToString()!);
        }

        private static ExportedBooleanCell? ProcessCheckboxColumn(object item, Binding binding)
        {
            var boundProperty = binding.Path.Path;

            var propertyInfo = item.GetType().GetProperty(boundProperty);
            var propertyValue = propertyInfo?.GetValue(item);

            if (propertyValue is null)
            {
                return null;
            }

            return new ExportedBooleanCell(propertyValue.ToString()!);
        }

        private static ExportedCell? ProcessComboboxColumn(object item, Binding binding, string displayMemberPath)
        {
            var boundProperty = binding.Path.Path;

            var itemsSourceType = item.GetType().GetProperty(boundProperty);
            var itemsSourceBoundProperty = itemsSourceType?.GetValue(item);

            if (itemsSourceBoundProperty is null)
            {
                return null;
            }

            var innerType = itemsSourceBoundProperty.GetType().GetProperty(displayMemberPath);
            var innerTypeProperty = innerType?.GetValue(itemsSourceBoundProperty);

            return new ExportedCell(innerTypeProperty.ToString()!);
        }

        private static DataGridCell GetCell(int row, int column, DataGrid dataGrid)
        {
            var rowContainer = GetRow(row, dataGrid);

            var presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);

            // Check if the DataGrid is virtualised, if it is, we will need to scroll into view
            // before grabbing the UI element, as it has not yet rendered on screen, and thus
            // is not available to us yet
            if (presenter?.ItemContainerGenerator.ContainerFromIndex(column) is not DataGridCell cell)
            {
                dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
                cell = (DataGridCell)presenter?.ItemContainerGenerator.ContainerFromIndex(column)!;
            }

            return cell;
        }

        private static DataGridRow GetRow(int index, DataGrid dataGrid)
        {
            // Check if the DataGrid is virtualised, if it is, we will need to scroll into view
            // before grabbing the UI element, as it has not yet rendered on screen, and thus
            // is not available to us yet
            if (dataGrid.ItemContainerGenerator.ContainerFromIndex(index) is not DataGridRow row)
            {
                dataGrid.ScrollIntoView(dataGrid.Items[index]);
                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(index);
            }

            return row;
        }

        private static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T dependencyObject)
                    return dependencyObject;

                var childOfChild = FindVisualChild<T>(child);

                return childOfChild;
            }

            return null;
        }
    }

    public class ExportBehaviour
    {
        public static readonly DependencyProperty DataGridExportProperty = 
            DependencyProperty.RegisterAttached("DataGridExport", typeof(object), typeof(ExportBehaviour), new PropertyMetadata(null));

        public static object? GetDataGridExport(DataGridColumn column)
        {
            var f = column.GetValue(DataGridExportProperty);

            return f;
        }

        public static void SetDataGridExport(DataGridColumn column, string value)
        {
            column.SetValue(DataGridExportProperty, value);
        }
    }
}
