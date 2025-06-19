using APP.Eds.Components.PopUp;
using APP.Eds.Helpers;
using APP.Eds.Models.Court;
using APP.Eds.Models.Dispenser;
using APP.Eds.Models.Eds;
using APP.Eds.Models.Hose;
using APP.Eds.Models.Inventory;
using APP.Eds.Models.Islander;
using APP.Eds.Models.Translations;
using APP.Eds.Services.Config;
using APP.Eds.UsesCases.Court;
using CommunityToolkit.Maui.Views;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Input;


namespace APP.Eds.Services.Court
{
    public class CourtService : INotifyPropertyChanged

    {
        
        private static CourtService _instance;
        public static CourtService Instance => _instance ??= new CourtService();
        private string? _authToken;

        public static void ResetInstanceFields()
        {
            _instance._authToken = null;
            _instance.SelectedBusiness = null;
            _instance.SelectedEds = null;
            _instance.SelectedIslander = null;
            _instance.TotalAmount = 0;
            _instance.TotalGallons = 0;
            _instance.TotalExpenditure = 0;
            _instance.TotalTypeOfCollection = 0;
            _instance.TotalSales = 0;
            _instance.Distintic = 0;
            _instance.CourtDispensers = null;
            _instance.CourtDocuments = null;
            _instance.CourtExpenditures = null;
            _instance.CourtTypeOfCollections = null;
            _instance.AdditionalInformation = null;
            
        }


        public ObservableCollection<EdsCourtModel> EdsList { get; set; } = [];
        public ObservableCollection<EdsCourtModel> EdsSelectList { get; set; } = [];
        public ObservableCollection<ProductCourtModel> ProductList { get; set; } = [];
        public ObservableCollection<CompartimentCourtModel> CompartimentList { get; set; } = [];
        public ObservableCollection<HoseCourtModel> HoseList { get; set; } = [];
        public ObservableCollection<HoseCourtModel> HoseDispenserList { get; set; } = [];
        public ObservableCollection<ExpendituresCourtModel> ExpenditureList { get; set; } = [];
        public ObservableCollection<TypeOfCollectionCourtModel> TypeOfCollectionList { get; set; } = [];
        public ObservableCollection<IslanderResponse> IslanderList { get; set; } = [];
        public ObservableCollection<IslanderResponse> IslanderSelectList { get; set; } = [];
        public ObservableCollection<BusinessModel> BusinessList { get; set; } = [];
        public ObservableCollection<DispenserModelResponse> DispensersList { get; set; } = [];
        public ObservableCollection<double> AmountResults { get; set; } = new ObservableCollection<double>();
        public ObservableCollection<double> GallonResults { get; set; } = new ObservableCollection<double>();
        public ObservableCollection<CourtListItemModel> CourtList { get; set; } = new();
        public bool AreAvailableHoses => HoseDispenserList != null && HoseDispenserList.Count > 0;
        public bool NewSaleEnabled => IsEdsSelected && AreAvailableHoses;

        private List<HoseCourtModel> selectedHoses = new List<HoseCourtModel>();


        private CourtModel _court;
        public CourtModel Court
        {
            get => _court;
            set
            {
                _court = value;
                OnPropertyChanged(nameof(Court));
            }
        }


        private int _idCourt;
        public int IdCourt
        {
            get => _idCourt;
            set
            {
                _idCourt = value;
                OnPropertyChanged(nameof(IdCourt));
            }
        }

        private bool _isIslanderLogin;
        public bool IsIslanderLogin
        {
            get => _isIslanderLogin;
            set
            {
                if (_isIslanderLogin != value)
                {
                    _isIslanderLogin = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotIslanderLogin));
                }
            }
        }

        public bool IsNotIslanderLogin => !IsIslanderLogin;


        private int _idIslander;
        public int IdIslander
        {
            get => _idIslander;
            set
            {
                _idIslander = value;
                OnPropertyChanged(nameof(IdIslander));
            }
        }

        private DateTime _dateStarttime;
        public DateTime DateStarttime
        {
            get => _dateStarttime;
            set
            {
                if (_dateStarttime != value)
                {
                    _dateStarttime = value;
                    OnPropertyChanged(nameof(DateStarttime));
                    UpdateDateEndtime();
                }
            }
        }

        private DateTime _dateEndtime;
        public DateTime DateEndtime
        {
            get => _dateEndtime;
            set
            {
                if (_dateEndtime != value)
                {
                    _dateEndtime = value;
                    OnPropertyChanged(nameof(DateEndtime));
                }
            }
        }


        private TimeSpan _startTime;
        public TimeSpan Starttime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged(nameof(Starttime));
                UpdateDateEndtime();
            }
        }

        private TimeSpan _endTime;
        public TimeSpan Endtime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged(nameof(Endtime));
                UpdateDateEndtime();
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

        private int _consecutive;
        public int Consecutive
        {
            get => _consecutive;
            set
            {
                _consecutive = value;
                OnPropertyChanged(nameof(Consecutive));
            }
        }

        private int _idEds;
        public int IdEds
        {
            get => _idEds;
            set
            {
                _idEds = value;
                OnPropertyChanged(nameof(IdEds));
            }
        }

       
        
        private int _idCourtDispenser;
        public int IdCourtDispenser
        {
            get => _idCourtDispenser;
            set
            {
                _idCourtDispenser = value;
                OnPropertyChanged(nameof(IdCourtDispenser));
            }
        }

        private int _idDocument;
        public int IdDocument
        {
            get => _idDocument;
            set
            {
                _idDocument = value;
                OnPropertyChanged(nameof(IdDocument));
            }
        }

        private string _documentDescription;
        public string DocumentDescription
        {
            get => _documentDescription;
            set
            {
                _documentDescription = value;
                OnPropertyChanged(nameof(DocumentDescription));
            }
        }

        private int _idExpenditure;
        public int IdExpenditure
        {
            get => _idExpenditure;
            set
            {
                _idExpenditure = value;
                OnPropertyChanged(nameof(IdExpenditure));
            }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        private string _expenditureDescription;
        public string ExpenditureDescription
        {
            get => _expenditureDescription;
            set
            {
                _expenditureDescription = value;
                OnPropertyChanged(nameof(ExpenditureDescription));
            }
        }

        private int _idType;
        public int IdType
        {
            get => _idType;
            set
            {
                _idType = value;
                OnPropertyChanged(nameof(IdType));
            }
        }

        private string _typeName;
        public string TypeName
        {
            get => _typeName;
            set
            {
                _typeName = value;
                OnPropertyChanged(nameof(TypeName));
            }
        }



        private int _idCourtTypeOfCollection;

        public int IdCourtTypeOfCollection
        {
            get => _idCourtTypeOfCollection;
            set
            {
                _idCourtTypeOfCollection = value;
                OnPropertyChanged(nameof(IdCourtTypeOfCollection));
            }
        }

        private int _courtTypeOfCollectionIdCourt;

        public int CourtTypeOfCollectionIdCourt
        {
            get => _courtTypeOfCollectionIdCourt;
            set
            {
                _courtTypeOfCollectionIdCourt = value;
                OnPropertyChanged(nameof(CourtTypeOfCollectionIdCourt));
            }
        }

        private string _courtTypeOfCollectionDescription;

        public string CourtTypeOfCollectionDescription
        {
            get => _courtTypeOfCollectionDescription;
            set
            {
                _courtTypeOfCollectionDescription = value;
                OnPropertyChanged(nameof(CourtTypeOfCollectionDescription));
            }
        }


        private int _idTypeOfCollection;

        public int IdTypeOfCollection
        {
            get => _idTypeOfCollection;
            set
            {
                _idTypeOfCollection = value;
                OnPropertyChanged(nameof(IdTypeOfCollection));
            }
        }

        private int _idBusiness;

        public int IdBusiness
        {
            get => _idBusiness;
            set
            {
                _idBusiness = value;
                OnPropertyChanged(nameof(IdBusiness));
            }
        }

        private int _idDispensers;

        public int IdDispensers
        {
            get => _idDispensers;
            set
            {
                _idDispensers = value;
                OnPropertyChanged(nameof(IdDispensers));
            }
        }

        private double _courtTypeOfCollectionAmount;

        public double CourtTypeOfCollectionAmount
        {
            get => _courtTypeOfCollectionAmount;
            set
            {
                _courtTypeOfCollectionAmount = value;
                OnPropertyChanged(nameof(CourtTypeOfCollectionAmount));
            }
        }
        private int _idCourtExpenditure;

        public int IdCourtExpenditure
        {
            get => _idCourtExpenditure;
            set
            {
                _idCourtExpenditure = value;
                OnPropertyChanged(nameof(IdCourtExpenditure));
            }
        }

        private int _courtExpenditureIdCourt;

        public int CourtExpenditureIdCourt
        {
            get => _courtExpenditureIdCourt;
            set
            {
                _courtExpenditureIdCourt = value;
                OnPropertyChanged(nameof(CourtExpenditureIdCourt));
            }
        }

        private int _idExpenditures;
        public int IdExpenditures
        {
            get => _idExpenditures;
            set
            {
                _idExpenditures = value;
                OnPropertyChanged(nameof(IdExpenditures));
            }
        }
        private double _courtExpenditureAmount;

        public double CourtExpenditureAmount
        {
            get => _courtExpenditureAmount;
            set
            {
                _courtExpenditureAmount = value;
                OnPropertyChanged(nameof(CourtExpenditureAmount));
            }
        }
        private int _courtDocumentIdCourt;

        public int CourtDocumentIdCourt
        {
            get => _courtDocumentIdCourt;
            set
            {
                _courtDocumentIdCourt = value;
                OnPropertyChanged(nameof(CourtDocumentIdCourt));
            }
        }
        private int _courtDispenserIdCourt;

        public int CourtDispenserIdCourt
        {
            get => _courtDispenserIdCourt;
            set
            {
                _courtDispenserIdCourt = value;
                OnPropertyChanged(nameof(CourtDispenserIdCourt));
            }
        }
        private double _accumulatedAmount;

        public double AccumulatedAmount
        {
            get => _accumulatedAmount;
            set
            {
                _accumulatedAmount = value;
                OnPropertyChanged(nameof(AccumulatedAmount));
                UpdateAmountDifferenceResult();
            }
        }

        private double _accumulatedGallons;

        public double AccumulatedGallons
        {
            get => _accumulatedGallons;
            set
            {
                _accumulatedGallons = value;
                OnPropertyChanged(nameof(AccumulatedGallons));
                UpdateGallonsDifferenceResult();
            }
        }

        private double _lastAccumulatedAmount;
        public double LastAccumulatedAmount
        {
            get => _lastAccumulatedAmount;
            set
            {
                _lastAccumulatedAmount = value;
                OnPropertyChanged(nameof(LastAccumulatedAmount));
                UpdateAmountDifferenceResult();
            }
        }

        private double _lastAccumulatedGallons;
        public double LastAccumulatedGallons
        {
            get => _lastAccumulatedGallons;
            set
            {
                _lastAccumulatedGallons = value;
                OnPropertyChanged(nameof(LastAccumulatedGallons));
                UpdateGallonsDifferenceResult();
            }
        }

        private double _amountDifferenceResult;
        public double AmountDifferenceResult
        {
            get => _amountDifferenceResult;
            set
            {
                _amountDifferenceResult = value;
                OnPropertyChanged(nameof(AmountDifferenceResult));
            }
        }

        private double _gallonsDifferenceResult;
        public double GallonsDifferenceResult
        {
            get => _gallonsDifferenceResult;
            set
            {
                _gallonsDifferenceResult = value;
                OnPropertyChanged(nameof(GallonsDifferenceResult));
            }
        }

        private double _totalAmount;
        public double TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private double _totalGallons;
        public double TotalGallons
        {
            get => _totalGallons;
            set
            {
                _totalGallons = value;
                OnPropertyChanged(nameof(TotalGallons));
            }
        }

        private double _totalExpenditure;
        public double TotalExpenditure
        {
            get => _totalExpenditure;
            set
            {
                _totalExpenditure = value;
                OnPropertyChanged(nameof(TotalExpenditure));
            }
        }

        private double _totalTypeOfCollection;
        public double TotalTypeOfCollection
        {
            get => _totalTypeOfCollection;
            set
            {
                _totalTypeOfCollection = value;
                OnPropertyChanged(nameof(TotalTypeOfCollection));
            }
        }

        private double _totalSales;
        public double TotalSales
        {
            get => _totalSales;
            set
            {
                _totalSales = value;
                OnPropertyChanged(nameof(TotalSales));
                OnPropertyChanged(nameof(IsTotalSalesValid));
            }
        }

        public bool IsTotalSalesValid => TotalSales >= 0;

        private int _dispenserNumber;
        public int DispenserNumber
        {
            get => _dispenserNumber;
            set
            {
                _dispenserNumber = value;
                OnPropertyChanged(nameof(DispenserNumber));
            }
        }

        private int _number;
        public int Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged(nameof(Number));
            }
        }

        private int _idProduct;
        public int IdProduct
        {
            get => _idProduct;
            set
            {
                _idProduct = value;
                OnPropertyChanged(nameof(IdProduct));
            }
        }
        private int _idCompartiment;
        public int IdCompartiment
        {
            get => _idCompartiment;
            set
            {
                _idCompartiment = value;
                OnPropertyChanged(nameof(IdCompartiment));
            }
        }
        private int _idHose;
        public int IdHose
        {
            get => _idHose;
            set
            {
                _idHose = value;
                OnPropertyChanged(nameof(IdHose));
            }
        }

        private int _distintic;
        public int Distintic
        {
            get => _distintic;
            set
            {
                _distintic = value;
                OnPropertyChanged(nameof(Distintic));
            }
        }        

        //Vista

        private bool _visibleLists = true;
        public bool VisibleLists
        {
            get => _visibleLists;
            set
            {
                _visibleLists = value;
                OnPropertyChanged(nameof(VisibleLists));
            }
        }

        private bool _visibleDispenser = true;
        public bool VisibleDispenser
        {
            get => _visibleDispenser;
            set
            {
                _visibleDispenser = value;
                OnPropertyChanged(nameof(VisibleDispenser));
            }
        }


        private bool _visibleDocuments = true;
        public bool VisibleDocuments
        {
            get => _visibleDocuments;
            set
            {
                _visibleDocuments = value;
                OnPropertyChanged(nameof(VisibleDocuments));
            }
        }

        private bool _visibleExpenses = true;
        public bool VisibleExpenses
        {
            get => _visibleExpenses;
            set
            {
                _visibleExpenses = value;
                OnPropertyChanged(nameof(VisibleExpenses));
            }
        }

        private bool _visibleReceipts = true;
        public bool VisibleReceipts
        {
            get => _visibleReceipts;
            set
            {
                _visibleReceipts = value;
                OnPropertyChanged(nameof(VisibleReceipts));
            }
        }


        //VALIDATION PICKER

        private bool _isBusinessSelected;
        public bool IsBusinessSelected
        {
            get => _isBusinessSelected;
            set
            {
                _isBusinessSelected = value;
                OnPropertyChanged(nameof(IsBusinessSelected));
            }
        }

        private bool _isEdsSelected;
        public bool IsEdsSelected
        {
            get => _isEdsSelected;
            set
            {
                _isEdsSelected = value;
                OnPropertyChanged(nameof(IsEdsSelected));
                OnPropertyChanged(nameof(NewSaleEnabled));
            }
        }

        //TRADUCCION

        private string _business = string.Empty;
        public string Business
        {
            get => _business;
            set
            {
                if (_business != value)
                {
                    _business = value;
                    OnPropertyChanged(nameof(Business));
                }
            }
        }

        private string _selectaBusiness = string.Empty;
        public string SelectaBusiness
        {
            get => _selectaBusiness;
            set
            {
                if (_selectaBusiness != value)
                {
                    _selectaBusiness = value;
                    OnPropertyChanged(nameof(SelectaBusiness));
                }
            }
        }


        private string _selectaEds = string.Empty;
        public string SelectaEds
        {
            get => _selectaEds;
            set
            {
                if (_selectaEds != value)
                {
                    _selectaEds = value;
                    OnPropertyChanged(nameof(SelectaEds));
                }
            }
        }

        private string _cuttingManagement = string.Empty;
        public string CuttingManagement
        {
            get => _cuttingManagement;
            set
            {
                if (_cuttingManagement != value)
                {
                    _cuttingManagement = value;
                    OnPropertyChanged(nameof(CuttingManagement));
                }
            }
        }


        private string _islander = string.Empty;
        public string Islander
        {
            get => _islander;
            set
            {
                if (_islander != value)
                {
                    _islander = value;
                    OnPropertyChanged(nameof(Islander));
                }
            }
        }

        private string _SelectAnIslander = string.Empty;
        public string SelectAnIslander
        {
            get => _SelectAnIslander;
            set
            {
                if (_SelectAnIslander != value)
                {
                    _SelectAnIslander = value;
                    OnPropertyChanged(nameof(SelectAnIslander));
                }
            }
        }

        private string _time = string.Empty;
        public string Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }
        }


        private string _DateTranslation = string.Empty;
        public string DateTranslation
        {
            get => _DateTranslation;
            set
            {
                if (_DateTranslation != value)
                {
                    _DateTranslation = value;
                    OnPropertyChanged(nameof(DateTranslation));
                }
            }
        }


        private string _StartTime = string.Empty;
        public string StartTime
        {
            get => _StartTime;
            set
            {
                if (_StartTime != value)
                {
                    _StartTime = value;
                    OnPropertyChanged(nameof(StartTime));
                }
            }
        }

        private string _EndTime = string.Empty;
        public string EndTime
        {
            get => _EndTime;
            set
            {
                if (_EndTime != value)
                {
                    _EndTime = value;
                    OnPropertyChanged(nameof(EndTime));
                }
            }
        }

        private string _AdditionalInformation = string.Empty;
        public string AdditionalInformation
        {
            get => _AdditionalInformation;
            set
            {
                if (_AdditionalInformation != value)
                {
                    _AdditionalInformation = value;
                    OnPropertyChanged(nameof(AdditionalInformation));
                }
            }
        }

        private string _DescriptionTranslation = string.Empty;
        public string DescriptionTranslation
        {
            get => _DescriptionTranslation;
            set
            {
                if (_DescriptionTranslation != value)
                {
                    _DescriptionTranslation = value;
                    OnPropertyChanged(nameof(DescriptionTranslation));
                }
            }
        }
        private string _EnterADescription = string.Empty;
        public string EnterADescription
        {
            get => _EnterADescription;
            set
            {
                if (_EnterADescription != value)
                {
                    _EnterADescription = value;
                    OnPropertyChanged(nameof(EnterADescription));
                }
            }
        }

        private string _DistinticDescription = string.Empty;
        public string DistinticDescription
        {
            get => _DistinticDescription;
            set
            {
                if (_DistinticDescription != value)
                {
                    _DistinticDescription = value;
                    OnPropertyChanged(nameof(DistinticDescription));
                }
            }
        }

        private string _CashCount = string.Empty;
        public string CashCount
        {
            get => _CashCount;
            set
            {
                if (_CashCount != value)
                {
                    _CashCount = value;
                    OnPropertyChanged(nameof(CashCount));
                }
            }
        }

        private string _SalesInGallons = string.Empty;
        public string SalesInGallons
        {
            get => _SalesInGallons;
            set
            {
                if (_SalesInGallons != value)
                {
                    _SalesInGallons = value;
                    OnPropertyChanged(nameof(SalesInGallons));
                }
            }
        }

        private string _SalesInMoney = string.Empty;
        public string SalesInMoney
        {
            get => _SalesInMoney;
            set
            {
                if (_SalesInMoney != value)
                {
                    _SalesInMoney = value;
                    OnPropertyChanged(nameof(SalesInMoney));
                }
            }
        }

        private string _AddedDispensers = string.Empty;
        public string AddedDispensers
        {
            get => _AddedDispensers;
            set
            {
                if (_AddedDispensers != value)
                {
                    _AddedDispensers = value;
                    OnPropertyChanged(nameof(AddedDispensers));
                }
            }
        }

        private string _AddedDocuments = string.Empty;
        public string AddedDocuments
        {
            get => _AddedDocuments;
            set
            {
                if (_AddedDocuments != value)
                {
                    _AddedDocuments = value;
                    OnPropertyChanged(nameof(AddedDocuments));
                }
            }
        }

        private string _AddedExpenses = string.Empty;
        public string AddedExpenses
        {
            get => _AddedExpenses;
            set
            {
                if (_AddedExpenses != value)
                {
                    _AddedExpenses = value;
                    OnPropertyChanged(nameof(AddedExpenses));
                }
            }
        }

        private string _AddDispenser = string.Empty;
        public string AddDispenser
        {
            get => _AddDispenser;
            set
            {
                if (_AddDispenser != value)
                {
                    _AddDispenser = value;
                    OnPropertyChanged(nameof(AddDispenser));
                }
            }
        }

        private string _addDocumentTranslation = string.Empty;
        public string AddDocumentTranslation
        {
            get => _addDocumentTranslation;
            set
            {
                if (_addDocumentTranslation != value)
                {
                    _addDocumentTranslation = value;
                    OnPropertyChanged(nameof(AddDocumentTranslation));
                }
            }
        }

        private string _AddExpense = string.Empty;
        public string AddExpense
        {
            get => _AddExpense;
            set
            {
                if (_AddExpense != value)
                {
                    _AddExpense = value;
                    OnPropertyChanged(nameof(AddExpense));
                }
            }
        }

        private string _AddCollectionType = string.Empty;
        public string AddCollectionType
        {
            get => _AddCollectionType;
            set
            {
                if (_AddCollectionType != value)
                {
                    _AddCollectionType = value;
                    OnPropertyChanged(nameof(AddCollectionType));
                }
            }
        }

        private string _SendData = string.Empty;
        public string SendData
        {
            get => _SendData;
            set
            {
                if (_SendData != value)
                {
                    _SendData = value;
                    OnPropertyChanged(nameof(SendData));
                }
            }
        }

        private string _SelectAHose = string.Empty;
        public string SelectAHose
        {
            get => _SelectAHose;
            set
            {
                if (_SelectAHose != value)
                {
                    _SelectAHose = value;
                    OnPropertyChanged(nameof(SelectAHose));
                }
            }
        }

        private string _Add = string.Empty;
        public string Add
        {
            get => _Add;
            set
            {
                if (_Add != value)
                {
                    _Add = value;
                    OnPropertyChanged(nameof(Add));
                }
            }
        }

        private string _Select = string.Empty;
        public string Select
        {
            get => _Select;
            set
            {
                if (_Select != value)
                {
                    _Select = value;
                    OnPropertyChanged(nameof(Select));
                }
            }
        }

        private string _Expenditures = string.Empty;
        public string Expenditures
        {
            get => _Expenditures;
            set
            {
                if (_Expenditures != value)
                {
                    _Expenditures = value;
                    OnPropertyChanged(nameof(Expenditures));
                }
            }
        }

        private string _AmountTranslation = string.Empty;
        public string AmountTranslation
        {
            get => _AmountTranslation;
            set
            {
                if (_AmountTranslation != value)
                {
                    _AmountTranslation = value;
                    OnPropertyChanged(nameof(AmountTranslation));
                }
            }
        }

        private string _CollectionAmount = string.Empty;
        public string CollectionAmount
        {
            get => _CollectionAmount;
            set
            {
                if (_CollectionAmount != value)
                {
                    _CollectionAmount = value;
                    OnPropertyChanged(nameof(CollectionAmount));
                }
            }
        }

        private string _EnterthedescriptionTranslations = string.Empty;
        public string EnterthedescriptionTranslations
        {
            get => _EnterthedescriptionTranslations;
            set
            {
                if (_EnterthedescriptionTranslations != value)
                {
                    _EnterthedescriptionTranslations = value;
                    OnPropertyChanged(nameof(EnterthedescriptionTranslations));
                }
            }
        }


        private string _SelectFile = string.Empty;
        public string SelectFile
        {
            get => _SelectFile;
            set
            {
                if (_SelectFile != value)
                {
                    _SelectFile = value;
                    OnPropertyChanged(nameof(SelectFile));
                }
            }
        }

        private string _ShiftClosing = string.Empty;
        public string ShiftClosing
        {
            get => _ShiftClosing;
            set
            {
                if (_ShiftClosing != value)
                {
                    _ShiftClosing = value;
                    OnPropertyChanged(nameof(ShiftClosing));
                }
            }
        }

        private string _TypesofAggregateCollections = string.Empty;
        public string TypesofAggregateCollections
        {
            get => _TypesofAggregateCollections;
            set
            {
                if (_TypesofAggregateCollections != value)
                {
                    _TypesofAggregateCollections = value;
                    OnPropertyChanged(nameof(TypesofAggregateCollections));
                }
            }
        }

        private string _Eds = string.Empty;
        public string Eds
        {
            get => _Eds;
            set
            {
                if (_Eds != value)
                {
                    _Eds = value;
                    OnPropertyChanged(nameof(Eds));
                }
            }
        }

        private string _Expenses = string.Empty;
        public string Expenses
        {
            get => _Expenses;
            set
            {
                if (_Expenses != value)
                {
                    _Expenses = value;
                    OnPropertyChanged(nameof(Expenses));
                }
            }
        }

        private string _Collections = string.Empty;
        public string Collections
        {
            get => _Collections;
            set
            {
                if (_Collections != value)
                {
                    _Collections = value;
                    OnPropertyChanged(nameof(Collections));
                }
            }
        }

        private string _TotalForTheDay = string.Empty;
        public string TotalForTheDay
        {
            get => _TotalForTheDay;
            set
            {
                if (_TotalForTheDay != value)
                {
                    _TotalForTheDay = value;
                    OnPropertyChanged(nameof(TotalForTheDay));
                }
            }
        }

        private string _Lists = string.Empty;
        public string Lists
        {
            get => _Lists;
            set
            {
                if (_Lists != value)
                {
                    _Lists = value;
                    OnPropertyChanged(nameof(Lists));
                }
            }
        }

        private string _NumberTranslation = string.Empty;
        public string NumberTranslation
        {
            get => _NumberTranslation;
            set
            {
                if (_NumberTranslation != value)
                {
                    _NumberTranslation = value;
                    OnPropertyChanged(nameof(NumberTranslation));
                }
            }
        }

        private string _AccumulatedAmountTranslation = string.Empty;
        public string AccumulatedAmountTranslation
        {
            get => _AccumulatedAmountTranslation;
            set
            {
                if (_AccumulatedAmountTranslation != value)
                {
                    _AccumulatedAmountTranslation = value;
                    OnPropertyChanged(nameof(_AccumulatedAmountTranslation));
                }
            }
        }

        private string _GallonsAccumulatedTranslation = string.Empty;
        public string GallonsAccumulatedTranslation
        {
            get => _GallonsAccumulatedTranslation;
            set
            {
                if (_GallonsAccumulatedTranslation != value)
                {
                    _GallonsAccumulatedTranslation = value;
                    OnPropertyChanged(nameof(GallonsAccumulatedTranslation));
                }
            }
        }



        private string _GallonsAccumulated = string.Empty;
        public string GallonsAccumulated
        {
            get => _GallonsAccumulated;
            set
            {
                if (_GallonsAccumulated != value)
                {
                    _GallonsAccumulated = value;
                    OnPropertyChanged(nameof(GallonsAccumulated));
                }
            }
        }

        private string _Document = string.Empty;
        public string Document
        {
            get => _Document;
            set
            {
                if (_Document != value)
                {
                    _Document = value;
                    OnPropertyChanged(nameof(Document));
                }
            }
        }

        private string _Expenditure = string.Empty;
        public string Expenditure
        {
            get => _Expenditure;
            set
            {
                if (_Expenditure != value)
                {
                    _Expenditure = value;
                    OnPropertyChanged(nameof(Expenditure));
                }
            }
        }

        private string _NewSale = string.Empty;
        public string NewSale
        {
            get => _NewSale;
            set
            {
                if (_NewSale != value)
                {
                    _NewSale = value;
                    OnPropertyChanged(nameof(NewSale));
                }
            }
        }

        private string _UploadProofOfPayment = string.Empty;
        public string UploadProofOfPayment
        {
            get => _UploadProofOfPayment;
            set
            {
                if (_UploadProofOfPayment != value)
                {
                    _UploadProofOfPayment = value;
                    OnPropertyChanged(nameof(UploadProofOfPayment));
                }
            }
        }

        private string _NewEgress = string.Empty;
        public string NewEgress
        {
            get => _NewEgress;
            set
            {
                if (_NewEgress != value)
                {
                    _NewEgress = value;
                    OnPropertyChanged(nameof(NewEgress));
                }
            }
        }

        private string _AccumulatedGallonsTranslations = string.Empty;
        public string AccumulatedGallonsTranslations
        {
            get => _AccumulatedGallonsTranslations;
            set
            {
                if (AccumulatedGallonsTranslations != value)
                {
                    _AccumulatedGallonsTranslations = value;
                    OnPropertyChanged(nameof(AccumulatedGallons));
                }
            }
        }

        private string _EnterTheGallons = string.Empty;
        public string EnterTheGallons
        {
            get => _EnterTheGallons;
            set
            {
                if (_EnterTheGallons != value)
                {
                    _EnterTheGallons = value;
                    OnPropertyChanged(nameof(EnterTheGallons));
                }
            }
        }

        private string _HoseTranslation = string.Empty;
        public string HoseTranslation
        {
            get => _HoseTranslation;
            set
            {
                if (_HoseTranslation != value)
                {
                    _HoseTranslation = value;
                    OnPropertyChanged(nameof(HoseTranslation));
                }
            }
        }
        private string _LastAccumulatedGallonsTranslation = string.Empty;
        public string LastAccumulatedGallonsTranslation
        {
            get => _LastAccumulatedGallonsTranslation;
            set
            {
                if (_LastAccumulatedGallonsTranslation != value)
                {
                    _LastAccumulatedGallonsTranslation = value;
                    OnPropertyChanged(nameof(LastAccumulatedGallonsTranslation));
                }
            }
        }

        private string _LastAccumulatedAmountTranslation = string.Empty;
        public string LastAccumulatedAmountTranslation
        {
            get => _LastAccumulatedAmountTranslation;
            set
            {
                if (_LastAccumulatedAmountTranslation != value)
                {
                    _LastAccumulatedAmountTranslation = value;
                    OnPropertyChanged(nameof(LastAccumulatedAmountTranslation));
                }
            }
        }



        private string _TypeofCollectionTranslation = string.Empty;
        public string TypeofCollectionTranslation
        {
            get => _TypeofCollectionTranslation;
            set
            {
                if (_TypeofCollectionTranslation != value)
                {
                    _TypeofCollectionTranslation = value;
                    OnPropertyChanged(nameof(TypeofCollectionTranslation));
                }
            }
        } 
        
        private string _CategoryTranslation = string.Empty;

        public string CategoryTranslation
        {
            get => _CategoryTranslation;
            set
            {
                if (_CategoryTranslation != value)
                {
                    _CategoryTranslation = value;
                    OnPropertyChanged(nameof(CategoryTranslation));
                }
            }
        }

        private string _EnterCategory = string.Empty;

        public string EnterCategory
        {
            get => _EnterCategory;
            set
            {
                if (_EnterCategory != value)
                {
                    _EnterCategory = value;
                    OnPropertyChanged(nameof(EnterCategory));
                }
            }
        }

        private EdsCourtModel _selectedEds;
        public EdsCourtModel SelectedEds
        {
            get => _selectedEds;
            set
            {
                _selectedEds = value;
                OnPropertyChanged(nameof(SelectedEds));

                IsEdsSelected = _selectedEds != null;

                if (_selectedEds != null)
                {
                    IdEds = _selectedEds.IdEds;
                    LoadHoseByEds(IdEds);
                }
            }
        }

        private IslanderResponse _selectedIslander;
        public IslanderResponse SelectedIslander
        {
            get => _selectedIslander;
            set
            {
                _selectedIslander = value;
                OnPropertyChanged(nameof(SelectedIslander));
                if (_selectedIslander != null)
                {
                    IdIslander = _selectedIslander.IdIslander;
                }
            }
        }
        private ProductCourtModel _selectedProduct;
        public ProductCourtModel SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
                if (_selectedProduct != null)
                {
                    IdProduct = _selectedProduct.IdProduct;
                }
            }
        }


        private CompartimentCourtModel _selectedCompartiment;
        public CompartimentCourtModel SelectedCompartiment
        {
            get => _selectedCompartiment;
            set
            {
                _selectedCompartiment = value;
                OnPropertyChanged(nameof(SelectedCompartiment));
                if (_selectedCompartiment != null)
                {
                    IdCompartiment = _selectedCompartiment.IdCompartiment;
                }
            }
        }

        private HoseCourtModel _selectedHose;
        public HoseCourtModel SelectedHose
        {
            get => _selectedHose;
            set
            {
                _selectedHose = value;
                OnPropertyChanged(nameof(SelectedHose));
                if (_selectedHose != null)
                {
                    IdHose = _selectedHose.IdHose;
                    LoadLastAccumulated(_selectedHose.IdDispensers, _selectedHose.IdHose);
                }
            }
        }

        private ExpendituresCourtModel _selectedExpenditure;
        public ExpendituresCourtModel SelectedExpenditure
        {
            get => _selectedExpenditure;
            set
            {
                _selectedExpenditure = value;
                OnPropertyChanged(nameof(SelectedExpenditure));
                if (_selectedExpenditure != null)
                {
                    IdCourtExpenditure = _selectedExpenditure.IdCourtExpenditure;
                }
            }
        }

        private TypeOfCollectionCourtModel _selectedTypeOfCollection;
        public TypeOfCollectionCourtModel SelectedTypeOfCollection
        {
            get => _selectedTypeOfCollection;
            set
            {
                _selectedTypeOfCollection = value;
                OnPropertyChanged(nameof(SelectedTypeOfCollection));
                if (_selectedTypeOfCollection != null)
                {
                    IdTypeOfCollection = _selectedTypeOfCollection.IdTypeOfCollection;
                }
            }
        }

      
        private BusinessModel _selectedBusiness;
        public BusinessModel SelectedBusiness
        {
            get => _selectedBusiness;
            set
            {
                _selectedBusiness = value;
                OnPropertyChanged(nameof(SelectedBusiness));

                if (_selectedBusiness != null)
                {
                    IdBusiness = _selectedBusiness.IdBusiness;
                }
            }
        }



        private ObservableCollection<CourtExpenditure> _courtExpenditures;
        public ObservableCollection<CourtExpenditure> CourtExpenditures
        {
            get => _courtExpenditures;
            set
            {
                _courtExpenditures = value;
                OnPropertyChanged(nameof(CourtExpenditures));
            }
        }

        private ObservableCollection<CourtDispenser> _courtDispensers;
        public ObservableCollection<CourtDispenser> CourtDispensers
        {
            get => _courtDispensers;
            set
            {
                _courtDispensers = value;
                OnPropertyChanged(nameof(CourtDispensers));
            }
        }

        private ObservableCollection<CourtLastAccumulated> _lastAccumulated;
        public ObservableCollection<CourtLastAccumulated> LastAccumulated
        {
            get => _lastAccumulated;
            set
            {
                _lastAccumulated = value;
                OnPropertyChanged(nameof(LastAccumulated));
            }
        }

        private ObservableCollection<CourtDocument> _courtDocuments;
        public ObservableCollection<CourtDocument> CourtDocuments
        {
            get => _courtDocuments;
            set
            {
                _courtDocuments = value;
                OnPropertyChanged(nameof(CourtDocuments));
            }
        }

        private ObservableCollection<CourtTypeOfCollection> _courtTypeOfCollections;
        public ObservableCollection<CourtTypeOfCollection> CourtTypeOfCollections
        {
            get => _courtTypeOfCollections;
            set
            {
                _courtTypeOfCollections = value;
                OnPropertyChanged(nameof(CourtTypeOfCollections));
            }
        }

        private ObservableCollection<CourtBusiness> _courtBusiness;
        public ObservableCollection<CourtBusiness> CourtBusiness
        {
            get => _courtBusiness;
            set
            {
                _courtBusiness = value;
                OnPropertyChanged(nameof(CourtBusiness));
            }
        }

        private ObservableCollection<CourtDispenser> _courtDispenser;
        public ObservableCollection<CourtDispenser> CourtDispenser
        {
            get => _courtDispenser;
            set
            {
                _courtDispenser = value;
                OnPropertyChanged(nameof(CourtDispenser));
            }
        }

        public ICommand LoadCourtDataCommand { get; }
        public ICommand SendCourtDataCommand { get; }
        public ICommand OpenCourtDetailCommand => new Command<CourtListItemModel>(OpenCourtDetail);
        public Command HideLists { get; }
        public Command HideListDispenser { get; }
        public Command HideDocumentList { get; }
        public Command HideExpenseList { get; }
        public Command HideCollectionList { get; }

        public ICommand DeleteDispenserCommand => new Command<CourtDispenser>(DeleteDispenser);
        public ICommand DeleteDocumentCommand => new Command<CourtDocument>(DeleteDocument);
        public ICommand DeleteExpenseCommand => new Command<CourtExpenditure>(DeleteExpense);
        public ICommand DeleteCollectionCommand => new Command<CourtTypeOfCollection>(DeleteCollection);

        public async Task LoadTranslationsAsync()
        {
            try
            {
                var result = await GetTranslationsByLanguageAsync("es-CO");
                GlobalTranslations.SetTranslations(result ?? []);

                Business = GlobalTranslations.Get("Business");
                SelectaBusiness = GlobalTranslations.Get("SelectABusiness");
                SelectaEds = GlobalTranslations.Get("SelectAEDS");
                CuttingManagement = GlobalTranslations.Get("CuttingManagement");
                Islander = GlobalTranslations.Get("Islander");
                SelectAnIslander = GlobalTranslations.Get("SelectAnIslander");
                Time = GlobalTranslations.Get("Time");
                DateTranslation = GlobalTranslations.Get("Date");
                StartTime = GlobalTranslations.Get("StartTime");
                EndTime = GlobalTranslations.Get("EndTime");
                AdditionalInformation = GlobalTranslations.Get("AdditionalInformation");
                DescriptionTranslation = GlobalTranslations.Get("Description");
                EnterADescription = GlobalTranslations.Get("EnterADescription");
                DistinticDescription = GlobalTranslations.Get("DistinticDescription");
                CashCount = GlobalTranslations.Get("CashCount");
                SalesInGallons = GlobalTranslations.Get("SalesInGallons");
                SalesInMoney = GlobalTranslations.Get("SalesInMoney");
                AddedDispensers = GlobalTranslations.Get("AddedDispensers");
                AddedDocuments = GlobalTranslations.Get("AddedDocuments");
                AddedExpenses = GlobalTranslations.Get("AddedExpenses");
                TypesofAggregateCollections = GlobalTranslations.Get("TypesOfAggregateCollections");
                AddDispenser = GlobalTranslations.Get("AddDispenser");
                AddDocumentTranslation = GlobalTranslations.Get("AddDocument");
                AddExpense = GlobalTranslations.Get("AddExpense");
                AddCollectionType = GlobalTranslations.Get("AddCollectionType");
                SendData = GlobalTranslations.Get("SendData");
                SelectAHose = GlobalTranslations.Get("SelectAHose");
                Add = GlobalTranslations.Get("Add");
                SelectFile = GlobalTranslations.Get("SelectFile");
                Select = GlobalTranslations.Get("Select");
                Expenditures = GlobalTranslations.Get("Expenditures");
                AmountTranslation = GlobalTranslations.Get("Amount");
                CollectionAmount = GlobalTranslations.Get("CollectionAmount");
                EnterthedescriptionTranslations = GlobalTranslations.Get("EnterThedescription");
                ShiftClosing = GlobalTranslations.Get("ShiftClosing");
                Eds = GlobalTranslations.Get("Eds");
                Expenses = GlobalTranslations.Get("Expenses");
                Collections = GlobalTranslations.Get("Collections");
                TotalForTheDay = GlobalTranslations.Get("TotalForTheDay");
                Lists = GlobalTranslations.Get("Lists");
                NumberTranslation = GlobalTranslations.Get("Number");
                AccumulatedAmountTranslation = GlobalTranslations.Get("AccumulatedAmount");
                GallonsAccumulatedTranslation = GlobalTranslations.Get("GallonsAccumulated");
                Document = GlobalTranslations.Get("Document");
                Expenditure = GlobalTranslations.Get("Expenditure");
                NewSale = GlobalTranslations.Get("NewSale");
                UploadProofOfPayment = GlobalTranslations.Get("UploadProofOfPayment");
                NewEgress = GlobalTranslations.Get("NewEgress");
                AccumulatedGallonsTranslations = GlobalTranslations.Get("AccumulatedGallons");
                EnterTheGallons = GlobalTranslations.Get("EnterTheGallons");
                HoseTranslation = GlobalTranslations.Get("Hose");
                TypeofCollectionTranslation = GlobalTranslations.Get("TypeofCollection");
                LastAccumulatedGallonsTranslation = GlobalTranslations.Get("LastAccumulatedGallons");
                LastAccumulatedAmountTranslation = GlobalTranslations.Get("LastAccumulatedAmount");
                CategoryTranslation = GlobalTranslations.Get("Category");
                EnterCategory = GlobalTranslations.Get("EnterCategory");
        
            }
            catch (Exception ex)
            {
                GlobalTranslations.SetTranslations([]);
            }
        }

        public CourtService()
        {
            _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            //OcultarListas

            HideLists = new Command(() =>
            {
                VisibleLists = !VisibleLists;

            });

            HideListDispenser = new Command(() =>
            {
                VisibleDispenser = !VisibleDispenser;

            });

            HideDocumentList = new Command(() =>
            {
                VisibleDocuments = !VisibleDocuments;
            });

            HideExpenseList = new Command(() =>
            {

                VisibleExpenses = !VisibleExpenses;
            });

            HideCollectionList = new Command(() =>
            {
                VisibleReceipts = !VisibleReceipts;
            });
        _authToken = TokenHelper.LoadToken(Configuration.KeycloakCliendId, Configuration.KeycloakRealms);
            GetAllEdsData();
            DateStarttime = DateTime.Now;
            DateEndtime = DateTime.Now;
            Starttime = new TimeSpan(6, 0, 0);
            Endtime = new TimeSpan(18, 0, 0);
            LoadCourtDataCommand = new Command<int>(async (courtId) => await LoadCourtDataAsync(courtId));
            SendCourtDataCommand = new Command(async () => await SendCourtDataAsync());
            CourtExpenditures = new ObservableCollection<CourtExpenditure>();
            IsIslanderLogin = false;
            
        }

        public async 
        Task
GetAllEdsData()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            
            try
            {
                
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var IslanderResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/islander?PageNumber=1&PageSize=100");
                var edsResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/eds?PageNumber=1&PageSize=100");
                var productResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/product?PageNumber=1&PageSize=100");
                var compartimentResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/compartiment?PageNumber=1&PageSize=100");
                var hoseResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/hose?PageNumber=1&PageSize=100");
                var expenditureResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/expenditures?PageNumber=1&PageSize=100");
                var typeOfCollectionResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/type-of-collection?PageNumber=1&PageSize=100");
                var businessResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/business?PageNumber=1&PageSize=100");
                var dispensersResponse = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/dispensers?PageNumber=1&PageSize=100");
               

                var IslanderList = JsonSerializer.Deserialize<IslanderApiResponse>(IslanderResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var edsList = JsonSerializer.Deserialize<EdsCourtResponseModel>(edsResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var productList = JsonSerializer.Deserialize<ProductCourtResponseModel>(productResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var compartimentList = JsonSerializer.Deserialize<CompartimentCourtResponseModel>(compartimentResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var hoseList = JsonSerializer.Deserialize<HoseCourtResponseModel>(hoseResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var expenditureList = JsonSerializer.Deserialize<ExpenditureCourtResponseModel>(expenditureResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var typeOfCollectionList = JsonSerializer.Deserialize<TypeOfCollectionResponseModel>(typeOfCollectionResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var businessList = JsonSerializer.Deserialize<BusinessResponseModel>(businessResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var dispensersList = JsonSerializer.Deserialize<DispensersResponseModel>(dispensersResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

               
                UpdateEdsList(edsList?.Data ?? new List<EdsCourtModel>());
                UpdateIslanderList(IslanderList?.Data ?? new List<IslanderResponse>());
                UpdateProductList(productList?.Data ?? new List<ProductCourtModel>());
                UpdateCompartiment(compartimentList?.Data ?? new List<CompartimentCourtModel>());
                UpdateHose(hoseList?.Data ?? new List<HoseCourtModel>());
                UpdateCourtExpenditure(expenditureList?.Data ?? new List<ExpendituresCourtModel>());
                UpdateTypeOfCollection(typeOfCollectionList?.Data ?? new List<TypeOfCollectionCourtModel>());
                UpdateBusiness(businessList?.Data ?? new List<BusinessModel>());
                UpdateDispensers(dispensersList?.Data ?? new List<DispenserModelResponse>());

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando los datos: {ex.Message}");


            }
        }

        private async void LoadLastAccumulated(int idDispenser, int idHose)
        {
            if (idDispenser <= 0 || idHose <= 0)
            {
                throw new ArgumentException("idDispenser and idHose must be greater than 0.");
            }

            var selectedHose = HoseList.FirstOrDefault(h => h.IdDispensers == idDispenser && h.IdHose == idHose);
            if (selectedHose != null)
            {
                LastAccumulatedAmount = selectedHose.AccumulatedAmount;
                LastAccumulatedGallons = selectedHose.AccumulatedGallons;
            }
        }

        private void AddAmountDifferenceResult(double accumulatedAmount, double lastAccumulatedAmount)
        {
            AmountDifferenceResult = accumulatedAmount - lastAccumulatedAmount;
            AmountResults.Add(AmountDifferenceResult);
        }

        private void AddGallonsDifferenceResult(double accumulatedGallons, double lastAccumulatedGallons)
        {
            GallonsDifferenceResult = accumulatedGallons - lastAccumulatedGallons;
            GallonResults.Add(GallonsDifferenceResult);
        }

        public void UpdateAmountDifferenceResult()
        {
            if(AccumulatedAmount >= LastAccumulatedAmount)
            {
                AmountDifferenceResult = AccumulatedAmount - LastAccumulatedAmount;
            }
            else
            {
                AmountDifferenceResult = 0;
            }
        }

        public void UpdateGallonsDifferenceResult()
        {
            if(AccumulatedGallons >= LastAccumulatedGallons)
            {
                GallonsDifferenceResult = AccumulatedGallons - LastAccumulatedGallons;
            }
            else
            {
                GallonsDifferenceResult = 0;
            }
        }

        private void UpdateDispensers(IEnumerable<DispenserModelResponse> dispensers)
        {
            DispensersList.Clear();
            foreach (var Dispensers in dispensers)
            {
                DispensersList.Add(Dispensers);
            }
        }
        private void UpdateIslanderList(IEnumerable<IslanderResponse> islanders)
        {
            IslanderList.Clear();
            foreach (var islander in islanders)
            {
                IslanderList.Add(islander);
            }
        }

        private void UpdateEdsList(IEnumerable<EdsCourtModel> edsData)
        {
            EdsList.Clear();
            foreach (var eds in edsData)
            {
                EdsList.Add(eds);
            }
        }

        private void UpdateProductList(IEnumerable<ProductCourtModel> productData)
        {
            ProductList.Clear();
            foreach (var product in productData)
            {
                ProductList.Add(product);
            }
        }

        private void UpdateCompartiment(IEnumerable<CompartimentCourtModel> compartimentData)
        {
            CompartimentList.Clear();
            foreach (var compartiment in compartimentData)
            {
                CompartimentList.Add(compartiment);
            }
        }

        private void UpdateHose(IEnumerable<HoseCourtModel> hoseData)
        {
            HoseList.Clear();
            foreach (var hose in hoseData)
            {
                HoseList.Add(hose);
            }

        }

        private void UpdateCourtExpenditure(IEnumerable<ExpendituresCourtModel> expenditureData)
        {
            ExpenditureList.Clear();
            foreach (var expenditure in expenditureData)
            {
                ExpenditureList.Add(expenditure);
            }
        }

        private void UpdateTypeOfCollection(IEnumerable<TypeOfCollectionCourtModel> typeOfCollectionData)
        {
            TypeOfCollectionList.Clear();
            foreach (var typeOfCollection in typeOfCollectionData)
            {
                TypeOfCollectionList.Add(typeOfCollection);
            }
        }

        private void UpdateBusiness(IEnumerable<BusinessModel> businessData)
        {
            BusinessList.Clear();
            foreach (var business in businessData)
            {
                BusinessList.Add(business);
            }
        }

        private void UpdateDateEndtime()
        {
            if (Endtime < Starttime)
                DateEndtime = DateStarttime.AddDays(1);
            else
                DateEndtime = DateStarttime;
        }

        public async Task<Dictionary<string, string>> GetTranslationsByLanguageAsync(string languageTag)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return new Dictionary<string, string>();
            }
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/translations");
            var data = JsonSerializer.Deserialize<TranslationsResponse>(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return data.Translations.TryGetValue(languageTag, out var translations)
                ? translations
                : new Dictionary<string, string>();

        }

        public async Task LoadCourtDataAsync(int courtId)
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/court/{courtId}");
                Console.WriteLine(response);

                Court = JsonSerializer.Deserialize<CourtModel>(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar el dato: {ex.Message}", "OK");
            }
        }
        
        public async Task LoadAllCourtListAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var response = await httpClient.GetStringAsync($"{Configuration.BaseUrl}/api/v1/court?PageNumber=1&PageSize=100");
                var courts = JsonSerializer.Deserialize<List<CourtListItemModel>>(response, new JsonSerializerOptions{PropertyNameCaseInsensitive = true});

                CourtList.Clear();

                if (courts != null)
                {
                    foreach (var court in courts)
                    {
                        CourtList.Add(court);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo cargar la lista de courts: {ex.Message}", "OK");
            }
        }

        private void OpenCourtDetail(CourtListItemModel selectedCourt)
        {
            if (selectedCourt == null) return;

            Application.Current.MainPage.ShowPopup(new CourtDetailPopup(selectedCourt));
        }



        public async Task AddDispenserFromPopup()
        { 
            if (Court == null)
            {
                Court = new CourtModel();
            }

            if (CourtDispensers == null)
            {
                CourtDispensers = new ObservableCollection<CourtDispenser>();
            }

            var newDispenser = new CourtDispenser
            {
               
                DispenserNumber = SelectedHose.IdDispensers,
                NumberName = SelectedHose.Number,
                AccumulatedAmount = AccumulatedAmount,
                AccumulatedGallons = AccumulatedGallons,
                LastAccumulatedAmount = LastAccumulatedAmount,
                LastAccumulatedGallons = LastAccumulatedGallons,
                AmountDifferenceResult = AmountDifferenceResult,
                GallonsDifferenceResult = GallonsDifferenceResult,
                IdHose = IdHose
            };

            CourtDispensers.Add(newDispenser);
            Court.CourtDispensers = CourtDispensers.ToList();

            AddAmountDifferenceResult(AccumulatedAmount, LastAccumulatedAmount);
            AddGallonsDifferenceResult(AccumulatedGallons, LastAccumulatedGallons);

            TotalSales = GetTotalSales();
        }

        public void AddDocumentsFromPopup(List<string> filesBase64, List<string> nombresDocumentos)
        {
            if (Court == null)
                Court = new CourtModel();

            if (CourtDocuments == null)
                CourtDocuments = new ObservableCollection<CourtDocument>();

            for (int i = 0; i < filesBase64.Count; i++)
            {
                var newDocument = new CourtDocument
                {
                    Descripcion = filesBase64[i], // base64 temporalmente
                    DocumentName = nombresDocumentos[i],
                };

                CourtDocuments.Add(newDocument);
            }

            Court.CourtDocuments = CourtDocuments.ToList();
        }



        public async Task AddCourtExpenditureFromPopup()
        {
            if (Court == null)
            {
                Court = new CourtModel();
            }

            if (CourtExpenditures == null)
            {
                CourtExpenditures = new ObservableCollection<CourtExpenditure>();
            }

            var newCourtExpenditure = new CourtExpenditure
            {
                ExpenditureName = SelectedExpenditure.Description ?? string.Empty,
                Amount = CourtExpenditureAmount,
                Description = ExpenditureDescription,
                IdExpenditure =SelectedExpenditure.IdExpenditure,
            };

            CourtExpenditures.Add(newCourtExpenditure);
            Court.CourtExpenditures = CourtExpenditures.ToList();

            TotalSales = GetTotalSales();
            CourtExpenditureAmount = 0;
            ExpenditureDescription = "";
        }


        public async Task AddCourtTypeOfCollectionFromPopup()
        {
            if (Court == null)
            {
                Court = new CourtModel();
            }

            if (CourtTypeOfCollections == null)
            {
                CourtTypeOfCollections = new ObservableCollection<CourtTypeOfCollection>();
            }
            var newCourtTypeOfCollection = new CourtTypeOfCollection
            {
                TypeOfCollectionName = SelectedTypeOfCollection.Description ?? string.Empty,
                Amount = CourtTypeOfCollectionAmount,
                Description = CourtTypeOfCollectionDescription,
                IdTypeOfCollection = SelectedTypeOfCollection.IdTypeOfCollection,
            };

            CourtTypeOfCollections.Add(newCourtTypeOfCollection);
            Court.CourtTypeOfCollections = CourtTypeOfCollections.ToList();

            TotalSales = GetTotalSales();
            CourtTypeOfCollectionAmount = 0;
            CourtTypeOfCollectionDescription = "";
        }

        public double GetTotalAmount()
        {
            if (AmountResults == null || !AmountResults.Any())
                return 0;

            return AmountResults.Sum();
        }

        public double GetTotalGallons()
        {
            if (GallonResults == null || !GallonResults.Any())
                return 0;

            return GallonResults.Sum();
        }

        public double GetTotalExpenditure()
        {
            if (CourtExpenditures == null || !CourtExpenditures.Any())
                return 0;

            return CourtExpenditures.Sum(item => item.Amount);
        }      

        public double GetTotalTypeOfCollection()
        {
            if (CourtTypeOfCollections == null || !CourtTypeOfCollections.Any())
                return 0;

            return CourtTypeOfCollections.Sum(item => item.Amount);
        }

        public double GetTotalSales()
        {
            TotalAmount = GetTotalAmount();
            TotalGallons = GetTotalGallons();
            TotalExpenditure = GetTotalExpenditure();
            TotalTypeOfCollection = GetTotalTypeOfCollection();

            return TotalAmount - TotalExpenditure;
        }

        public async Task SendCourtDataAsync()
        {
            if (string.IsNullOrEmpty(_authToken))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se encontró el token de autenticación", "OK");
                return;
            }
            try
            {
                double totalAmmount = GetTotalAmount();
                double totalGallons = GetTotalGallons();
                double totalTypeOfCollection = GetTotalTypeOfCollection();
                double totalExpenditures = GetTotalExpenditure();

                if (totalAmmount != totalTypeOfCollection)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"La suma de los tipos de cobro no coincide con el total del día", "OK");
                    return;
                }

                double cash = totalTypeOfCollection - totalExpenditures;
                if (cash < 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"El total de efectivo no puede ser negativo", "OK");
                    return;
                }

                if (Court == null)
                {
                    Court = new CourtModel();
                }

                Court.IdBusiness = IdBusiness;
                Court.IdEds = IdEds;
                Court.IdIslander = IdIslander;
                Court.DateStarttime = DateStarttime.ToString("yyyy-MM-dd");
                Court.Starttime = Starttime.ToString(@"hh\:mm\:ss");
                Court.DateEndtime = DateEndtime.ToString("yyyy-MM-dd");
                Court.Endtime = Endtime.ToString(@"hh\:mm\:ss");
                Court.Descripcion = Description;
                Court.Distintic = Distintic;
                Court.CourtDocuments = CourtDocuments?.ToList();

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                var json = JsonSerializer.Serialize(Court, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync($"{Configuration.BaseUrl}/api/v1/court", content);

                if (response.IsSuccessStatusCode)
                {
                    if (CourtDocuments?.Any() == true)
                    {
                        string apiUrl = $"{Configuration.BaseUrl}/api/v1/files/upload";

                        using var client = new HttpClient();

                        foreach (var doc in CourtDocuments)
                        {
                            try
                            {
                                byte[] fileBytes = Convert.FromBase64String(doc.Descripcion);
                                using var fileStream = new MemoryStream(fileBytes);
                                using var contentFile = new MultipartFormDataContent();
                                var fileContent = new StreamContent(fileStream);
                                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                                contentFile.Add(fileContent, "files", doc.DocumentName);

                                HttpResponseMessage fileResponse = await client.PostAsync(apiUrl, contentFile);
                                if (!fileResponse.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($"Error al subir archivo: {doc.DocumentName} - {fileResponse.StatusCode}");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error subiendo {doc.DocumentName}: {ex.Message}");
                            }
                        }
                    }

                    await Application.Current.MainPage.DisplayAlert("Éxito", "Datos y documentos enviados correctamente", "OK");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo enviar el dato: {response.StatusCode}\n{error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar los datos: {ex.Message}", "OK");
            }
        }


        public void LoadEdsByBusiness(int businessId)
        {
          
            var filteredEds = EdsList.Where(x => x.IdBusiness == businessId).ToList();
            EdsSelectList.Clear();
            foreach (var eds in filteredEds)
            {
                EdsSelectList.Add(eds);
            }
            OnPropertyChanged(nameof(EdsSelectList));
        }

        public void LoadIslandersByEds(int edsId)
        {            
            var filteredIslanders = IslanderList.Where(x => x.IdEds == edsId).ToList();
            IslanderSelectList.Clear();
            foreach (var islander in filteredIslanders)
            {
                IslanderSelectList.Add(islander);
            }
            OnPropertyChanged(nameof(IslanderSelectList));
        }

        public void LoadHoseByEds(int edsId)
        {
            var filteredIsHoseByEds = HoseList
                .Where(x => x.EdsEntity.IdEds == edsId)
                .OrderBy(x => x.IdDispensers)
                .ThenBy(x => x.Number) 
                .ToList();

            HoseDispenserList.Clear();
            foreach (var hose in filteredIsHoseByEds)
            {
                HoseDispenserList.Add(hose);
            }
            OnPropertyChanged(nameof(HoseDispenserList));
            OnPropertyChanged(nameof(AreAvailableHoses));
            OnPropertyChanged(nameof(NewSaleEnabled));
        }



        public void AddSelectedHose(HoseCourtModel hose)
        {
            if (hose != null && !selectedHoses.Contains(hose))
            {
                selectedHoses.Add(hose);
                UpdateAvailableHoses();
            }
        }
        private void UpdateAvailableHoses()
        {
            var filteredHoses = HoseList.Where(h => !selectedHoses.Contains(h)).ToList();
            HoseDispenserList.Clear();
            foreach (var hose in filteredHoses)
            {
                HoseDispenserList.Add(hose);
            }
            OnPropertyChanged(nameof(HoseDispenserList));
            OnPropertyChanged(nameof(AreAvailableHoses));
            OnPropertyChanged(nameof(NewSaleEnabled));
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DeleteDispenser(CourtDispenser dispenser)
        {
            if (dispenser != null && CourtDispensers?.Contains(dispenser) == true)
            {
                var hose = HoseList.FirstOrDefault(h => h.IdHose == dispenser.IdHose);

                CourtDispensers.Remove(dispenser);

                if (hose != null)
                {
                    var selectedHose = selectedHoses.FirstOrDefault(h => h.IdHose == hose.IdHose);
                    if (selectedHose != null)
                    {
                        selectedHoses.Remove(selectedHose);
                    }

                    if (!HoseDispenserList.Contains(hose))
                    {
                        HoseDispenserList.Add(hose);
                        OnPropertyChanged(nameof(HoseDispenserList));
                        OnPropertyChanged(nameof(AreAvailableHoses));
                    }
                }

                TotalSales = GetTotalSales();
                OnPropertyChanged(nameof(CourtDispensers));
            }
        }

        private void DeleteDocument(CourtDocument document)
        {
            if (document != null && CourtDocuments.Contains(document))
            {
                CourtDocuments.Remove(document);
                OnPropertyChanged(nameof(CourtDocuments));
            }
        }

        private void DeleteExpense(CourtExpenditure expense)
        {
            if (expense != null && CourtExpenditures.Contains(expense))
            {
                CourtExpenditures.Remove(expense);
                TotalSales = GetTotalSales();
                OnPropertyChanged(nameof(CourtExpenditures));
            }
        }

        private void DeleteCollection(CourtTypeOfCollection collection)
        {
            if (collection != null && CourtTypeOfCollections.Contains(collection))
            {
                CourtTypeOfCollections.Remove(collection);
                TotalSales = GetTotalSales();
                OnPropertyChanged(nameof(CourtTypeOfCollections));
            }
        }
    }
}
