// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Xunit;

namespace Avalonia.Controls.UnitTests
{
    public class ItemsControlTests
    {
        [Fact]
        public void Should_Use_ItemTemplate_To_Create_Control()
        {
            var target = new ItemsControl
            {
                Template = GetTemplate(),
                ItemTemplate = new FuncDataTemplate<string>(_ => new Canvas()),
            };

            target.Items = new[] { "Foo" };
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            Assert.IsType<Canvas>(target.Presenter.Panel.Children[0]);
        }

        [Fact]
        public void Panel_Should_Have_TemplatedParent_Set_To_ItemsControl()
        {
            var target = new ItemsControl();

            target.Template = GetTemplate();
            target.Items = new[] { "Foo" };
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            Assert.Equal(target, target.Presenter.Panel.TemplatedParent);
        }

        [Fact]
        public void Item_Should_Have_TemplatedParent_Set_To_Null()
        {
            var target = new ItemsControl();

            target.Template = GetTemplate();
            target.Items = new[] { "Foo" };
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            var item = (TextBlock)target.Presenter.Panel.GetVisualChildren().First();

            Assert.Null(item.TemplatedParent);
        }

        [Fact]
        public void Control_Item_Should_Be_Logical_Child_Before_ApplyTemplate()
        {
            var target = new ItemsControl();
            var child = new Control();

            target.Template = GetTemplate();
            target.Items = new[] { child };

            Assert.Equal(child.Parent, target);
            Assert.Equal(child.GetLogicalParent(), target);
            Assert.Equal(new[] { child }, target.GetLogicalChildren());
        }

        [Fact]
        public void Control_Item_Should_Be_Removed_From_Logical_Children_Before_ApplyTemplate()
        {
            var target = new ItemsControl();
            var child = new Control();
            var items = new AvaloniaList<Control>(child);

            target.Template = GetTemplate();
            target.Items = items;
            items.RemoveAt(0);

            Assert.Null(child.Parent);
            Assert.Null(child.GetLogicalParent());
            Assert.Empty(target.GetLogicalChildren());
        }

        [Fact]
        public void Clearing_Items_Should_Clear_Child_Controls_Parent_Before_ApplyTemplate()
        {
            var target = new ItemsControl();
            var child = new Control();

            target.Template = GetTemplate();
            target.Items = new[] { child };
            target.Items = null;

            Assert.Null(child.Parent);
            Assert.Null(((ILogical)child).LogicalParent);
        }

        [Fact]
        public void Clearing_Items_Should_Clear_Child_Controls_Parent()
        {
            var target = new ItemsControl();
            var child = new Control();

            target.Template = GetTemplate();
            target.Items = new[] { child };
            target.ApplyTemplate();
            target.Items = null;

            Assert.Null(child.Parent);
            Assert.Null(((ILogical)child).LogicalParent);
        }

        [Fact]
        public void Adding_Control_Item_Should_Make_Control_Appear_In_LogicalChildren()
        {
            var target = new ItemsControl();
            var child = new Control();

            target.Template = GetTemplate();
            target.Items = new[] { child };

            // Should appear both before and after applying template.
            Assert.Equal(new ILogical[] { child }, target.GetLogicalChildren());

            target.ApplyTemplate();

            Assert.Equal(new ILogical[] { child }, target.GetLogicalChildren());
        }

        [Fact]
        public void Adding_String_Item_Should_Make_TextBlock_Appear_In_LogicalChildren()
        {
            var target = new ItemsControl();
            var child = new Control();

            target.Template = GetTemplate();
            target.Items = new[] { "Foo" };
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            var logical = (ILogical)target;
            Assert.Equal(1, logical.LogicalChildren.Count);
            Assert.IsType<TextBlock>(logical.LogicalChildren[0]);
        }

        [Fact]
        public void Setting_Items_To_Null_Should_Remove_LogicalChildren()
        {
            var target = new ItemsControl();
            var child = new Control();

            target.Template = GetTemplate();
            target.Items = new[] { "Foo" };
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            Assert.NotEmpty(target.GetLogicalChildren());

            target.Items = null;

            Assert.Equal(new ILogical[0], target.GetLogicalChildren());
        }

        [Fact]
        public void Setting_Items_Should_Fire_LogicalChildren_CollectionChanged()
        {
            var target = new ItemsControl();
            var child = new Control();
            var called = false;

            target.Template = GetTemplate();
            target.ApplyTemplate();

            ((ILogical)target).LogicalChildren.CollectionChanged += (s, e) =>
                called = e.Action == NotifyCollectionChangedAction.Add;

            target.Items = new[] { child };

            Assert.True(called);
        }

        [Fact]
        public void Setting_Items_To_Null_Should_Fire_LogicalChildren_CollectionChanged()
        {
            var target = new ItemsControl();
            var child = new Control();
            var called = false;

            target.Template = GetTemplate();
            target.Items = new[] { child };
            target.ApplyTemplate();

            ((ILogical)target).LogicalChildren.CollectionChanged += (s, e) =>
                called = e.Action == NotifyCollectionChangedAction.Remove;

            target.Items = null;

            Assert.True(called);
        }

        [Fact]
        public void Changing_Items_Should_Fire_LogicalChildren_CollectionChanged()
        {
            var target = new ItemsControl();
            var child = new Control();
            var called = false;

            target.Template = GetTemplate();
            target.Items = new[] { child };
            target.ApplyTemplate();

            ((ILogical)target).LogicalChildren.CollectionChanged += (s, e) => called = true;

            target.Items = new[] { "Foo" };

            Assert.True(called);
        }

        [Fact]
        public void Adding_Items_Should_Fire_LogicalChildren_CollectionChanged()
        {
            var target = new ItemsControl();
            var items = new AvaloniaList<string> { "Foo" };
            var called = false;

            target.Template = GetTemplate();
            target.Items = items;
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            ((ILogical)target).LogicalChildren.CollectionChanged += (s, e) =>
                called = e.Action == NotifyCollectionChangedAction.Add;

            items.Add("Bar");

            Assert.True(called);
        }

        [Fact]
        public void Removing_Items_Should_Fire_LogicalChildren_CollectionChanged()
        {
            var target = new ItemsControl();
            var items = new AvaloniaList<string> { "Foo", "Bar" };
            var called = false;

            target.Template = GetTemplate();
            target.Items = items;
            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            ((ILogical)target).LogicalChildren.CollectionChanged += (s, e) =>
                called = e.Action == NotifyCollectionChangedAction.Remove;

            items.Remove("Bar");

            Assert.True(called);
        }

        [Fact]
        public void LogicalChildren_Should_Not_Change_Instance_When_Template_Changed()
        {
            var target = new ItemsControl()
            {
                Template = GetTemplate(),
            };

            var before = ((ILogical)target).LogicalChildren;

            target.Template = null;
            target.Template = GetTemplate();

            var after = ((ILogical)target).LogicalChildren;

            Assert.NotNull(before);
            Assert.NotNull(after);
            Assert.Same(before, after);
        }

        [Fact]
        public void Empty_Class_Should_Initially_Be_Applied()
        {
            var target = new ItemsControl()
            {
                Template = GetTemplate(),
            };

            Assert.True(target.Classes.Contains(":empty"));
        }

        [Fact]
        public void Empty_Class_Should_Be_Cleared_When_Items_Added()
        {
            var target = new ItemsControl()
            {
                Template = GetTemplate(),
                Items = new[] { 1, 2, 3 },
            };

            Assert.False(target.Classes.Contains(":empty"));
        }

        [Fact]
        public void Empty_Class_Should_Be_Set_When_Empty_Collection_Set()
        {
            var target = new ItemsControl()
            {
                Template = GetTemplate(),
                Items = new[] { 1, 2, 3 },
            };

            target.Items = new int[0];

            Assert.True(target.Classes.Contains(":empty"));
        }

        [Fact]
        public void Setting_Presenter_Explicitly_Should_Set_Item_Parent()
        {
            var target = new TestItemsControl();
            var child = new Control();

            var presenter = new ItemsPresenter
            {
                TemplatedParent = target,
                [~ItemsPresenter.ItemsProperty] = target[~ItemsControl.ItemsProperty],
            };

            presenter.ApplyTemplate();
            target.Presenter = presenter;
            target.Items = new[] { child };
            target.ApplyTemplate();

            Assert.Equal(target, child.Parent);
            Assert.Equal(target, ((ILogical)child).LogicalParent);
        }

        [Fact]
        public void DataContexts_Should_Be_Correctly_Set()
        {
            var items = new object[]
            {
                "Foo",
                new Item("Bar"),
                new TextBlock { Text = "Baz" },
                new ListBoxItem { Content = "Qux" },
            };

            var target = new ItemsControl
            {
                Template = GetTemplate(),
                DataContext = "Base",
                DataTemplates = new DataTemplates
                {
                    new FuncDataTemplate<Item>(x => new Button { Content = x })
                },
                Items = items,
            };

            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            var dataContexts = target.Presenter.Panel.Children
                .Cast<Control>()
                .Select(x => x.DataContext)
                .ToList();

            Assert.Equal(
                new object[] { items[0], items[1], "Base", "Base" },
                dataContexts);
        }

        [Fact]
        public void MemberSelector_Should_Select_Member()
        {
            var target = new ItemsControl
            {
                Template = GetTemplate(),
                Items = new[] { new Item("Foo"), new Item("Bar") },
                MemberSelector = new FuncMemberSelector<Item, string>(x => x.Value),
            };

            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            var text = target.Presenter.Panel.Children
                .Cast<TextBlock>()
                .Select(x => x.Text)
                .ToList();

            Assert.Equal(new[] { "Foo", "Bar" }, text);
        }

        [Fact]
        public void Control_Item_Should_Not_Be_NameScope()
        {
            var items = new object[]
            {
                new TextBlock(),
            };

            var target = new ItemsControl
            {
                Template = GetTemplate(),
                Items = items,
            };

            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            var item = target.Presenter.Panel.LogicalChildren[0];
            Assert.Null(NameScope.GetNameScope((TextBlock)item));
        }

        [Fact]
        public void DataTemplate_Created_Item_Should_Be_NameScope()
        {
            var items = new object[]
            {
                "foo",
            };

            var target = new ItemsControl
            {
                Template = GetTemplate(),
                Items = items,
            };

            target.ApplyTemplate();
            target.Presenter.ApplyTemplate();

            var item = target.Presenter.Panel.LogicalChildren[0];
            Assert.NotNull(NameScope.GetNameScope((TextBlock)item));
        }

        private class Item
        {
            public Item(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        private FuncControlTemplate GetTemplate()
        {
            return new FuncControlTemplate<ItemsControl>(parent =>
            {
                return new Border
                {
                    Background = new Media.SolidColorBrush(0xffffffff),
                    Child = new ItemsPresenter
                    {
                        Name = "PART_ItemsPresenter",
                        MemberSelector = parent.MemberSelector,
                        [~ItemsPresenter.ItemsProperty] = parent[~ItemsControl.ItemsProperty],
                    }
                };
            });
        }

        private class TestItemsControl : ItemsControl
        {
            public new IItemsPresenter Presenter
            {
                get { return base.Presenter; }
                set { base.Presenter = value; }
            }
        }
    }
}
