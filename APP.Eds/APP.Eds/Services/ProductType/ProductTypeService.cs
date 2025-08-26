
﻿using APP.Eds.Models.ProductType;
using System.ComponentModel;
using System.Text.Json;
using System.Text;
using System.Windows.Input;
using APP.Eds.Services.Config;
using APP.Eds.Helpers;
using System.Net.Http.Headers;
using System.Collections.ObjectModel;

namespace APP.Eds.Services.ProductType
{
    public class ProductTypeService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ProductTypeRequest Request { get; set; }
        private ProductTypeModel _productType;
        private string? _authToken;
        public ProductTypeModel ProductType
        {
            get => _productType;
            set
            {
                _productType = value;
                OnPropertyChanged(nameof(ProductType));
            }
        }

        private ObservableCollection<ProductTypeModel> _productTypeList = [];
        public ObservableCollection<ProductTypeModel> ProductTypeList
        {
            get => _productTypeList;
            set
            {
                _productTypeList = value;
                OnPropertyChanged(nameof(ProductTypeList));
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        public ICommand EditProductTypeCommand { get; }
        public ICommand DeleteProductTypeCommand { get; }
        public ICommand GetByIdProductTypeDataCommand { get; }

        public ProductTypeService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            EditProductTypeCommand = new Command<ProductTypeModel>(async (productType) => await EditProductType(productType));
            DeleteProductTypeCommand = new Command<ProductTypeModel>(async (productType) => await DeleteProductType(productType));
            GetByIdProductTypeDataCommand = new Command<int>(async (productTypeId) => await GetByIdProductTypeDataAsync(productTypeId));
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
