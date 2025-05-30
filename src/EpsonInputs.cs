﻿using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using System;
using System.Collections.Generic;

namespace EpsonProjectorEpi
{
    public class EpsonInputs : ISelectableItems<int>
    {
        private Dictionary<int, ISelectableItem> _items = new Dictionary<int, ISelectableItem>();

        public Dictionary<int, ISelectableItem> Items
        {
            get
            {
                return _items;
            }
            set
            {
                if (_items == value)
                    return;

                _items = value;

                ItemsUpdated?.Invoke(this, null);
            }
        }

        private int _currentItem;

        public int CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                if (_currentItem == value)
                    return;

                _currentItem = value;

                CurrentItemChanged?.Invoke(this, null);
            }
        }

        public event EventHandler ItemsUpdated;
        public event EventHandler CurrentItemChanged;

    }

    public class EpsonInput : ISelectableItem
    {
        private bool _isSelected;

        private readonly EpsonProjector _parent;

        private Action _inputMethod;

        public EpsonInput(string key, string name, EpsonProjector parent, Action inputMethod)
        {
            Key = key;
            Name = name;
            _parent = parent;
            _inputMethod = inputMethod;
        }

        public string Key { get; private set; }
        public string Name { get; private set; }

        public event EventHandler ItemUpdated;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
                var handler = ItemUpdated;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
        }

        public void Select()
        {
            _inputMethod();
        }
    }
}
