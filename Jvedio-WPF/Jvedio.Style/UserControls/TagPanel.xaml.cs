using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jvedio.Style.UserControls
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class TagPanel : UserControl
    {
        public delegate void ListChangedEventHandler(object sender, ListChangedEventArgs e);
        public event ListChangedEventHandler TagChanged;
        public event MouseButtonEventHandler onTagClick;
        public event RoutedEventHandler OnAddExistsLabel;
        public event RoutedEventHandler OnAddNewLabel;

        public static readonly DependencyProperty TagListProperty = DependencyProperty.Register(
"TagList", typeof(List<string>), typeof(TagPanel), new PropertyMetadata(null));

        public List<string> TagList
        {
            get { return (List<string>)GetValue(TagListProperty); }
            set
            {
                SetValue(TagListProperty, value);
            }
        }



        public TagPanel()
        {
            InitializeComponent();
            itemsControl.ItemsSource = null;
            itemsControl.ItemsSource = TagList;
            this.DataContext = this;
        }

        private void DeleteTag(object sender, MouseButtonEventArgs e)
        {
            StackPanel sp = (sender as FrameworkElement).Parent as StackPanel;
            TextBox textBox = sp.Children.OfType<TextBox>().First();
            var tl = TagList.Where(arg => arg == textBox.Text).ToList();
            foreach (var item in tl)
            {
                TagList.Remove(item);
            }
            TagChanged?.Invoke(this, new ListChangedEventArgs(TagList));
            Refresh();
        }

        private void DeleteLabel(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            Border border = (menuItem.Parent as ContextMenu).PlacementTarget as Border;
            Border border1 = (border.Child as StackPanel).Children.OfType<Border>().First();
            DeleteTag(border1, null);
        }

        public T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
        {
            T childElement = null;
            if (element == null) return childElement;
            var nChildCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < nChildCount; i++)
            {
                FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

                if (child == null)
                    continue;

                if (child is T && child.Name.Equals(sChildName))
                {
                    childElement = (T)child;
                    break;
                }

                childElement = FindElementByName<T>(child, sChildName);

                if (childElement != null)
                    break;
            }

            return childElement;
        }

        private void NewTag(object sender, RoutedEventArgs e)
        {
            OnAddNewLabel?.Invoke(this, null);

        }

        public void Refresh()
        {
            itemsControl.ItemsSource = null;
            itemsControl.ItemsSource = TagList;
            itemsControl.Items.Refresh();
        }

        private string GetTagName()
        {
            if (TagList == null) return "标签1";
            var tags = TagList.Where(arg => arg.IndexOf("标签") >= 0).ToList();
            if (tags.Count == 0) return "标签1";
            int max = 1;
            for (int i = 0; i < tags.Count; i++)
            {
                int.TryParse(tags[i].Replace("标签", ""), out int v);
                if (max < v) max = v;
            }
            max++;
            return "标签" + max;
        }

        private void SaveTag(object sender, RoutedEventArgs e)
        {
            //获得所有 Text;
            List<string> tags = new List<string>();
            for (int i = 0; i < itemsControl.Items.Count; i++)
            {
                ContentPresenter c = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromItem(itemsControl.Items[i]);
                WrapPanel wrapPanel = FindElementByName<WrapPanel>(c, "TagWrapPanel");
                if (wrapPanel != null)
                {
                    TextBox tb = LogicalTreeHelper.FindLogicalNode(wrapPanel, "tb") as TextBox;
                    string tag = tb.Text.Replace("；", "").Replace(";", "");
                    if (!tags.Contains(tag))
                        tags.Add(tag);
                }
            }
            TagChanged?.Invoke(this, new ListChangedEventArgs(tags));
        }

        private void tb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FocusTextBox.Focus();
            }
        }

        private void Tag_Click(object sender, MouseButtonEventArgs e)
        {
            onTagClick?.Invoke(sender, e);
        }

        private void AddExistsLabel(object sender, RoutedEventArgs e)
        {
            OnAddExistsLabel?.Invoke(sender, e);
        }


    }

    public class ListChangedEventArgs : EventArgs
    {
        public List<string> List;
        public ListChangedEventArgs(List<string> List)
        {
            this.List = List;
        }

        public ListChangedEventArgs() : this(new List<string>()) { }
    }
}
