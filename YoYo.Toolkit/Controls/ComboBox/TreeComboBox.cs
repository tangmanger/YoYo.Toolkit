using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YoYo.Toolkit.Controls.Interfaces;

namespace YoYo.Toolkit.Controls.ComboBox
{
    public class TreeComboBox : TreeView
    {


        public bool IsDorpDown
        {
            get { return (bool)GetValue(IsDorpDownProperty); }
            set { SetValue(IsDorpDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDorpDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDorpDownProperty =
            DependencyProperty.Register("IsDorpDown", typeof(bool), typeof(TreeComboBox), new PropertyMetadata(false));




        public bool IsOnlyLastNode
        {
            get { return (bool)GetValue(IsOnlyLastNodeProperty); }
            set { SetValue(IsOnlyLastNodeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsOnlyLastNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsOnlyLastNodeProperty =
            DependencyProperty.Register("IsOnlyLastNode", typeof(bool), typeof(TreeComboBox), new PropertyMetadata(false));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TreeComboBox), new PropertyMetadata(""));



        public ICommand SelectCommand
        {
            get { return (ICommand)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(TreeComboBox));


       
        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);

            SelectedItemUpdated();
            SelectCommand?.Execute(SelectedItem);

        }





      



       


        public TreeViewItem? GetTreeViewItem(TreeViewItem item)
        {
            var selectedTreeViewItem = item.ItemContainerGenerator.ContainerFromItem(this.SelectedItem) as TreeViewItem;
            if (selectedTreeViewItem != null)
            {
                return selectedTreeViewItem;
            }
            if (item.HasItems)
            {
                foreach (var childItem in item.Items)
                {
                    TreeViewItem? childView = item.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;
                    if (childView != null)
                    {
                        var data = GetTreeViewItem(childView);
                        if (data != null)
                        {
                            return data;
                        }
                    }
                }
            }
            return null;
        }

        internal void SelectedItemUpdated()
        {
            try
            {
                ITreeModel? treeModel = this.SelectedItem as ITreeModel;
                if (treeModel != null)
                {

                    if (IsOnlyLastNode)
                    {
                        if (!treeModel.HasChildren())
                        {

                            SetText(treeModel);
                        }
                        else
                        {
                            var treeViewItem = GetTreeViewItem();
                            if (treeViewItem != null)
                            {
                                treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                            }
                        }
                    }
                    else
                    {

                        SetText(treeModel);
                    }
                    return;
                }

                //if (selectedTreeViewItem != null)
                //{
                //    string? text = "";
                //    object header = selectedTreeViewItem.Header;
                //    if (header != null)
                //    {
                //        if (header is TextBlock)
                //        {
                //            var textBlock = (TextBlock)header;
                //            text = textBlock.Text;
                //        }
                //        if (header is Run)
                //        {
                //            var run = (Run)header;
                //            text = run.Text;
                //        }
                //        if (header is TextBox)
                //        {
                //            var textBox = (TextBox)header;
                //            text = textBox.Text;
                //        }
                //        if (header is ContentControl)
                //        {
                //            text = ((ContentControl)header)?.Content?.ToString();
                //        }
                //    }

                //    if (text != Text)
                //    {
                //        Text = text;
                //    }
                //}
                //else
                //{
                //    //Region region = (Region)GetValue(SelectedItemProperty);
                //    //if (region != null)
                //    //{
                //    //    Text = region.Name;
                //    //}
                //}
                //TextSearch.GetText
                //string text2 = this.SelectedValue?.ToString();
                //var selectedTreeViewItem = this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem) as TreeViewItem;
                //if (selectedTreeViewItem != null)
                //{
                //    string text = selectedTreeViewItem.Header?.ToString() ?? "";
                //    if (Text != text)
                //    {
                //        SetCurrentValue(TextProperty, text);
                //    }
                //}


                //  Update();
            }
            finally
            {
            }
        }

        private TreeViewItem? GetTreeViewItem()
        {
            var selectedTreeViewItem = this.ItemContainerGenerator.ContainerFromItem(this.SelectedItem) as TreeViewItem;
            if (selectedTreeViewItem == null)
            {
                foreach (var item in this.Items)
                {
                    TreeViewItem? treeViewItem = this.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                    if (treeViewItem != null)
                    {
                        selectedTreeViewItem = GetTreeViewItem(treeViewItem);
                        if (selectedTreeViewItem != null)
                            return selectedTreeViewItem;
                    }
                }
            }
            return selectedTreeViewItem;
        }

        private void SetText(ITreeModel treeModel)
        {
            IsDorpDown = false;
            if (Text != treeModel.DisplayName)
            {
                Text = treeModel.DisplayName;
            }
        }
    }
}
