using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace nosale.stackoverflow.WPF
{
    public static class ExpanderGroupBehavior
    {
        private static readonly Hashtable CurrentGroups = new Hashtable();

        public static DependencyProperty GroupNameProperty =
            DependencyProperty.RegisterAttached("GroupName", typeof(string), typeof(ExpanderGroupBehavior),
                new FrameworkPropertyMetadata(GroupNameChanged));

        public static void SetGroupName(Expander expander, string value)
        {
            expander.SetValue(GroupNameProperty, value);
        }

        public static string GetGroupName(Expander expander)
        {
            return (string) expander.GetValue(GroupNameProperty);
        }

        private static void GroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var expander = (Expander) d;
            var newGroupName = e.NewValue as string;
            var oldGroupName = e.OldValue as string;

            if (!string.IsNullOrEmpty(oldGroupName))
            {
                WeakEventManager<Expander, RoutedEventArgs>.RemoveHandler(expander, nameof(Expander.Expanded),
                    ExpanderExpanded);
                RemoveFromGroup(oldGroupName, expander);
            }

            if (!string.IsNullOrEmpty(newGroupName))
            {
                WeakEventManager<Expander, RoutedEventArgs>.AddHandler(expander, nameof(Expander.Expanded),
                    ExpanderExpanded);
                expander.SetCurrentValue(Expander.IsExpandedProperty,false);
                AddToGroup(newGroupName, expander);
            }
        }

        private static void AddToGroup(string groupName, Expander expander)
        {
            var currentElementsInGroup = (ArrayList) CurrentGroups[groupName];
            if (currentElementsInGroup == null)
            {
                currentElementsInGroup = new ArrayList();
                CurrentGroups[groupName] = currentElementsInGroup;
            }
            else
            {
                RemoveAndPurgeDead(currentElementsInGroup, null);
            }

            currentElementsInGroup.Add(new WeakReference(expander));
        }

        private static void RemoveFromGroup(string groupName, Expander expander)
        {
            var currentElementsInGroup = (ArrayList) CurrentGroups[groupName];
            if (currentElementsInGroup != null)
            {
                RemoveAndPurgeDead(currentElementsInGroup, expander);
                if (currentElementsInGroup.Count == 0) CurrentGroups.Remove(groupName);
            }
        }

        private static void RemoveAndPurgeDead(ArrayList elements, Expander expander)
        {
            for (var i = 0; i < elements.Count;)
            {
                var weakReference = (WeakReference) elements[i];
                var element = weakReference.Target as Expander;
                if (element == null || ReferenceEquals(element, expander))
                    elements.RemoveAt(i);
                else
                    i++;
            }
        }

        private static void ExpanderExpanded(object source, RoutedEventArgs args)
        {
            var expander = (Expander) source;
            HandleExpanderExpanded(expander);
        }

        private static void HandleExpanderExpanded(Expander expander)
        {
            var groupName = GetGroupName(expander);
            if (string.IsNullOrEmpty(groupName)) return;

            var elements = (ArrayList) CurrentGroups[groupName];
            for (var i = 0; i < elements.Count;)
            {
                var weakReference = (WeakReference) elements[i];
                var element = weakReference.Target as Expander;
                if (element == null)
                {
                    elements.RemoveAt(i);
                }
                else
                {
                    if (!ReferenceEquals(element, expander))
                    {
                        element.SetCurrentValue(Expander.IsExpandedProperty, false);
                    }
                    i++;
                }
            }
        }
    }
}