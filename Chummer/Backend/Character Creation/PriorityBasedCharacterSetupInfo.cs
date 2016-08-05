﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Chummer.Backend.Data.Infrastructure;
using Chummer.Backend.Data.Items;
using Chummer.Backend.Data.Sources;
using Chummer.Backend.Data.Sources.Xml;
using Chummer.Backend.Datastructures;

namespace Chummer.Backend.Character_Creation
{
	public class PriorityBasedCharacterSetupInfo : AbstractCharacterSetupInfo
	{
	    private readonly ICreationData _dataSource;
	    private int _karma;
		private int _bonusNuyen;
		private int _maxRating;
		private bool _ignoreRules;

		private readonly OptionListWrapper<GuidItem> _category = new OptionListWrapper<GuidItem>();
		private readonly OptionListWrapper<GuidItem> _metatype = new OptionListWrapper<GuidItem>();
		private readonly OptionListWrapper<GuidItem> _metavariant = new OptionListWrapper<GuidItem>();

		private readonly OptionListWrapper<GuidItem> _gameplayOption = new OptionListWrapper<GuidItem>();
	    private int _sumToTenValue = 10;
	    private CharacterBuildMethod _buildMethod;
	    private IReadOnlyList<MetatypeData.AttributesData> _attributes;

	    public int SumToTenValue
	    {
	        get { return _sumToTenValue; }
	        set { MaybeNotifyChanged(ref _sumToTenValue, value); }
	    }

	    internal PriorityBasedCharacterSetupInfo(ICreationData dataSource)
		{
	        _dataSource = dataSource;
	        _category.ListChangedEvent += CategoryOnListChangedEvent;
            _category.SelectedItemChangedEvent += CategoryOnSelectedItemChangedEvent;

            _metatype.ListChangedEvent += MetatypeOnListChangedEvent;
            _metatype.SelectedItemChangedEvent += MetatypeOnSelectedItemChangedEvent;

            _metavariant.ListChangedEvent += MetavariantOnListChangedEvent;
            _metavariant.SelectedItemChangedEvent += MetavariantOnSelectedItemChangedEvent;

            _gameplayOption.ListChangedEvent += GameplayOptionOnListChangedEvent;
            _gameplayOption.SelectedItemChangedEvent += GameplayOptionOnSelectedItemChangedEvent;

            _dataSource = dataSource;

		    foreach (GameplayOptionData data in _dataSource.GameplayOption)
		    {
		        GuidItem item = new GuidItem(data.DisplayName, data.ItemId);
                _gameplayOption.Add(item);

		        if (data.Default)
		        {
		            _gameplayOption.SelectedItem = item;
		        }
		    }

            _category.AddRange(_dataSource.Categories);
            _category.SelectedItem = _category[0];


		}

	    private void GameplayOptionOnSelectedItemChangedEvent(GuidItem selected)
	    {
	        GameplayOptionData option = _dataSource.GameplayOption[selected.Guid];
	        Karma = option.Karma;
	        MaxRating = option.MaxAvailability;

	    }

	    private void GameplayOptionOnListChangedEvent()
	    {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(GameplayOptionList)));
        }

        private void MetavariantOnSelectedItemChangedEvent(GuidItem selected)
	    {
	        throw new NotImplementedException();
	    }

	    private void MetavariantOnListChangedEvent()
	    {
	        OnPropertyChanged(new PropertyChangedEventArgs(nameof(MetavariantList)));
        }

	    private void MetatypeOnSelectedItemChangedEvent(GuidItem selected)
	    {
	        //throw new NotImplementedException();
            if(selected == null) return;
	        

	        MetatypeData data = _dataSource.Metatypes[selected.Guid];
	        _attributes = data.Attributes;
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Attributes)));
	    }

	    private void MetatypeOnListChangedEvent()
	    {
	        OnPropertyChanged(new PropertyChangedEventArgs(nameof(MetatypeList)));
	    }

	    private void CategoryOnSelectedItemChangedEvent(GuidItem selected)
	    {
            _metatype.ReplaceWith(_dataSource.Metatypes.Where(metatype => metatype.Categoryid == selected.Guid).Select(m => new GuidItem(m.DisplayName, m.Id)));
	        
	    }

	    private void CategoryOnListChangedEvent()
	    {
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(CategoryList)));
        }

        public override CharacterBuildMethod BuildMethod
	    {
	        get { return _buildMethod; }
	        set { _buildMethod = value; }
	    }

	    public override int Karma
		{
			get { return _karma; }
			set { MaybeNotifyChanged(ref _karma, value); }
		}

		public override int BonusNuyen
		{
			get { return _bonusNuyen; }
			set { MaybeNotifyChanged(ref _bonusNuyen, value); }
		}

		public override int MaxRating
		{
			get { return _maxRating; }
			set { MaybeNotifyChanged(ref _maxRating, value); }
		}

		public override bool IgnoreRules
		{
			get { return _ignoreRules; }
			set { MaybeNotifyChanged(ref _ignoreRules, value); }
		}

		public override GuidItem SelectedCategory
		{
			get { return _category.SelectedItem; }
		    set
		    {
		        if ((_category.SelectedItem == null && value != null) ||
                    (_category.SelectedItem != null && value == null) ||
                    (_category.SelectedItem != null && value != null && !value.Equals(_category.SelectedItem)))
                {
                    _category.SelectedItem = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedCategory)));
                }
            }
		}

		public override GuidItem SelectedMetatype
		{
			get { return _metatype.SelectedItem; }
		    set
		    {
                if ((_metatype.SelectedItem == null && value != null) ||
                    (_metatype.SelectedItem != null && value == null) ||
                    (_metatype.SelectedItem != null && value != null && !value.Equals(_metatype.SelectedItem)))
                {
                    _metatype.SelectedItem = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedMetatype)));
                }
            }
		}

		public override GuidItem SelectedMetavariant
		{
			get { return _metavariant.SelectedItem; }
		    set
		    {
		        if ((_metavariant.SelectedItem == null && value != null) ||
		            (_metavariant.SelectedItem != null && value == null) ||
		            (_metavariant.SelectedItem != null && value != null && !value.Equals(_metavariant.SelectedItem)))
		        {
		            _metavariant.SelectedItem = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedMetavariant)));
		        }
		    }
		}

		public override IReadOnlyCollection<GuidItem> CategoryList => _category.ReadOnly;

		public override IReadOnlyCollection<GuidItem> MetatypeList => _metatype.ReadOnly;

		public override IReadOnlyCollection<GuidItem> MetavariantList => _metavariant.ReadOnly;

		public override GuidItem SelectedGameplayOption
		{
			get { return _gameplayOption.SelectedItem; }
			set { _gameplayOption.SelectedItem = value; }
		}

		public override IReadOnlyCollection<GuidItem> GameplayOptionList => _gameplayOption.ReadOnly;

	    internal override IReadOnlyList<MetatypeData.AttributesData> Attributes
	    {
	        get { return _attributes; }
	    }


	    protected override IEnumerable<CharacterSetupAction> SetupActions()
		{
			throw new System.NotImplementedException();
		}

	    private void MaybeNotifyChanged<T>(ref T own, T value, [CallerMemberName] string name = "")
	    {
	        if ((own == null && value != null) || (own != null && value == null) || (own != null && value != null && !value.Equals(own))) 
	        {
                own = value;
                OnPropertyChanged(new PropertyChangedEventArgs(name));
	        }
	    }

    }
}