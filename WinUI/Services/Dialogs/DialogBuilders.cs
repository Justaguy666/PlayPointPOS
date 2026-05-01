using System;
using Application.Services;
using Application.Services.Areas;
using Application.Services.Members;
using Microsoft.UI.Xaml.Controls;
using WinUI.Services.Factories;
using WinUI.UIModels.Enums;
using WinUI.ViewModels.Dialogs;
using WinUI.ViewModels.Dialogs.Dashboard;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.Views.Dialogs;
using WinUI.Views.Dialogs.Dashboard;
using WinUI.Views.Dialogs.Management;

namespace WinUI.Services.Dialogs;

public sealed class ConfigDialogBuilder : ParameterlessDialogBuilder
{
    private readonly Func<ConfigViewModel> _createViewModel;

    public ConfigDialogBuilder(Func<ConfigViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.Config;

    protected override ContentDialog CreateCore()
        => new ConfigDialog(_createViewModel());
}

public sealed class RegisterDialogBuilder : ParameterlessDialogBuilder
{
    private readonly Func<RegisterViewModel> _createViewModel;

    public RegisterDialogBuilder(Func<RegisterViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.Register;

    protected override ContentDialog CreateCore()
        => new RegisterDialog(_createViewModel());
}

public sealed class LoginDialogBuilder : ParameterlessDialogBuilder
{
    private readonly Func<LoginViewModel> _createViewModel;

    public LoginDialogBuilder(Func<LoginViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.Login;

    protected override ContentDialog CreateCore()
        => new LoginDialog(_createViewModel());
}

public sealed class ForgotPasswordDialogBuilder : ParameterlessDialogBuilder
{
    private readonly Func<ForgotPasswordViewModel> _createViewModel;

    public ForgotPasswordDialogBuilder(Func<ForgotPasswordViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.ForgotPassword;

    protected override ContentDialog CreateCore()
        => new ForgotPasswordDialog(_createViewModel());
}

public sealed class OtpDialogBuilder : OptionalDialogBuilder<OtpDialogRequest>
{
    private readonly Func<OtpViewModel> _createViewModel;

    public OtpDialogBuilder(Func<OtpViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.Otp;

    protected override ContentDialog CreateCore(OtpDialogRequest? request)
    {
        OtpViewModel viewModel = _createViewModel();
        viewModel.Configure(request);
        return new OtpDialog(viewModel);
    }
}

public sealed class ReservationDialogBuilder : OptionalDialogBuilder<ReservationDialogRequest>
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizationPreferencesService _preferencesService;
    private readonly IMemberLookupService _memberLookupService;
    private readonly Func<IDialogService> _getDialogService;
    private readonly IAreaSessionService _areaSessionService;
    private readonly AreaModelFactory _areaModelFactory;

    public ReservationDialogBuilder(
        ILocalizationService localizationService,
        ILocalizationPreferencesService preferencesService,
        IMemberLookupService memberLookupService,
        Func<IDialogService> getDialogService,
        IAreaSessionService areaSessionService,
        AreaModelFactory areaModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _preferencesService = preferencesService ?? throw new ArgumentNullException(nameof(preferencesService));
        _memberLookupService = memberLookupService ?? throw new ArgumentNullException(nameof(memberLookupService));
        _getDialogService = getDialogService ?? throw new ArgumentNullException(nameof(getDialogService));
        _areaSessionService = areaSessionService ?? throw new ArgumentNullException(nameof(areaSessionService));
        _areaModelFactory = areaModelFactory ?? throw new ArgumentNullException(nameof(areaModelFactory));
    }

    public override DialogKey Key => DialogKey.Reservation;

    protected override ContentDialog CreateCore(ReservationDialogRequest? request)
    {
        request ??= new ReservationDialogRequest();
        var viewModel = new ReservationViewModel(
            _localizationService,
            _preferencesService,
            _memberLookupService,
            _getDialogService(),
            _areaSessionService,
            _areaModelFactory,
            request.Mode);

        return new ReservationDialog(viewModel, request);
    }
}

public sealed class StartSessionDialogBuilder : RequiredDialogBuilder<StartSessionDialogRequest>
{
    private readonly Func<StartSessionViewModel> _createViewModel;

    public StartSessionDialogBuilder(Func<StartSessionViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.StartSession;

    public override ContentDialog Create(StartSessionDialogRequest request)
        => new StartSessionDialog(_createViewModel(), request.Model);
}

public sealed class AreaFilterDialogBuilder : OptionalDialogBuilder<AreaFilterDialogRequest>
{
    private readonly Func<AreaFilterViewModel> _createViewModel;

    public AreaFilterDialogBuilder(Func<AreaFilterViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.AreaFilter;

    protected override ContentDialog CreateCore(AreaFilterDialogRequest? request)
        => new AreaFilterDialog(_createViewModel(), request);
}

public sealed class PaymentDialogBuilder : RequiredDialogBuilder<PaymentDialogRequest>
{
    private readonly Func<PaymentViewModel> _createViewModel;

    public PaymentDialogBuilder(Func<PaymentViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.Payment;

    public override ContentDialog Create(PaymentDialogRequest request)
        => new PaymentDialog(_createViewModel(), request.Model);
}

public sealed class AreaDialogBuilder : OptionalDialogBuilder<AreaDialogRequest>
{
    private readonly ILocalizationService _localizationService;
    private readonly Func<IDialogService> _getDialogService;
    private readonly AreaModelFactory _areaModelFactory;

    public AreaDialogBuilder(
        ILocalizationService localizationService,
        Func<IDialogService> getDialogService,
        AreaModelFactory areaModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _getDialogService = getDialogService ?? throw new ArgumentNullException(nameof(getDialogService));
        _areaModelFactory = areaModelFactory ?? throw new ArgumentNullException(nameof(areaModelFactory));
    }

    public override DialogKey Key => DialogKey.Area;

    protected override ContentDialog CreateCore(AreaDialogRequest? request)
    {
        var viewModel = new AreaDialogViewModel(
            _localizationService,
            _getDialogService(),
            _areaModelFactory,
            request?.Mode ?? UpsertDialogMode.Add);

        return new AreaDialog(viewModel, request);
    }
}

public sealed class GameFilterDialogBuilder : OptionalDialogBuilder<GameFilterDialogRequest>
{
    private readonly Func<GameFilterViewModel> _createViewModel;

    public GameFilterDialogBuilder(Func<GameFilterViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.GameFilter;

    protected override ContentDialog CreateCore(GameFilterDialogRequest? request)
        => new GameFilterDialog(_createViewModel(), request);
}

public sealed class GameTypeDialogBuilder : RequiredDialogBuilder<GameTypeDialogRequest>
{
    private readonly Func<GameTypeDialogViewModel> _createViewModel;

    public GameTypeDialogBuilder(Func<GameTypeDialogViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.GameType;

    public override ContentDialog Create(GameTypeDialogRequest request)
    {
        GameTypeDialogViewModel viewModel = _createViewModel();
        viewModel.Configure(request);
        return new GameTypeDialog(viewModel);
    }
}

public sealed class GameDialogBuilder : OptionalDialogBuilder<GameDialogRequest>
{
    private readonly ILocalizationService _localizationService;
    private readonly Func<IDialogService> _getDialogService;
    private readonly Func<IFilePickerService> _getFilePickerService;
    private readonly GameModelFactory _gameModelFactory;

    public GameDialogBuilder(
        ILocalizationService localizationService,
        Func<IDialogService> getDialogService,
        Func<IFilePickerService> getFilePickerService,
        GameModelFactory gameModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _getDialogService = getDialogService ?? throw new ArgumentNullException(nameof(getDialogService));
        _getFilePickerService = getFilePickerService ?? throw new ArgumentNullException(nameof(getFilePickerService));
        _gameModelFactory = gameModelFactory ?? throw new ArgumentNullException(nameof(gameModelFactory));
    }

    public override DialogKey Key => DialogKey.Game;

    protected override ContentDialog CreateCore(GameDialogRequest? request)
    {
        var viewModel = new GameDialogViewModel(
            _localizationService,
            _getDialogService(),
            _getFilePickerService(),
            _gameModelFactory,
            request?.Mode ?? UpsertDialogMode.Add);

        return new GameDialog(viewModel, request);
    }
}

public sealed class ProductFilterDialogBuilder : OptionalDialogBuilder<ProductFilterDialogRequest>
{
    private readonly Func<ProductFilterViewModel> _createViewModel;

    public ProductFilterDialogBuilder(Func<ProductFilterViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.ProductFilter;

    protected override ContentDialog CreateCore(ProductFilterDialogRequest? request)
        => new ProductFilterDialog(_createViewModel(), request);
}

public sealed class ProductDialogBuilder : OptionalDialogBuilder<ProductDialogRequest>
{
    private readonly ILocalizationService _localizationService;
    private readonly Func<IDialogService> _getDialogService;
    private readonly Func<IFilePickerService> _getFilePickerService;
    private readonly ProductModelFactory _productModelFactory;

    public ProductDialogBuilder(
        ILocalizationService localizationService,
        Func<IDialogService> getDialogService,
        Func<IFilePickerService> getFilePickerService,
        ProductModelFactory productModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _getDialogService = getDialogService ?? throw new ArgumentNullException(nameof(getDialogService));
        _getFilePickerService = getFilePickerService ?? throw new ArgumentNullException(nameof(getFilePickerService));
        _productModelFactory = productModelFactory ?? throw new ArgumentNullException(nameof(productModelFactory));
    }

    public override DialogKey Key => DialogKey.Product;

    protected override ContentDialog CreateCore(ProductDialogRequest? request)
    {
        var viewModel = new ProductDialogViewModel(
            _localizationService,
            _getDialogService(),
            _getFilePickerService(),
            _productModelFactory,
            request?.Mode ?? UpsertDialogMode.Add);

        return new ProductDialog(viewModel, request);
    }
}

public sealed class MemberDialogBuilder : OptionalDialogBuilder<MemberDialogRequest>
{
    private readonly ILocalizationService _localizationService;
    private readonly Func<IDialogService> _getDialogService;
    private readonly IMembershipRankManagementService _rankManagementService;
    private readonly MemberModelFactory _memberModelFactory;

    public MemberDialogBuilder(
        ILocalizationService localizationService,
        Func<IDialogService> getDialogService,
        IMembershipRankManagementService rankManagementService,
        MemberModelFactory memberModelFactory)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _getDialogService = getDialogService ?? throw new ArgumentNullException(nameof(getDialogService));
        _rankManagementService = rankManagementService ?? throw new ArgumentNullException(nameof(rankManagementService));
        _memberModelFactory = memberModelFactory ?? throw new ArgumentNullException(nameof(memberModelFactory));
    }

    public override DialogKey Key => DialogKey.Member;

    protected override ContentDialog CreateCore(MemberDialogRequest? request)
    {
        var viewModel = new MemberDialogViewModel(
            _localizationService,
            _getDialogService(),
            _rankManagementService,
            _memberModelFactory,
            request?.Mode ?? UpsertDialogMode.Add);

        return new MemberDialog(viewModel, request);
    }
}

public sealed class MemberFilterDialogBuilder : OptionalDialogBuilder<MemberFilterDialogRequest>
{
    private readonly Func<MemberFilterViewModel> _createViewModel;

    public MemberFilterDialogBuilder(Func<MemberFilterViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.MemberFilter;

    protected override ContentDialog CreateCore(MemberFilterDialogRequest? request)
        => new MemberFilterDialog(_createViewModel(), request);
}

public sealed class MembershipPackageDialogBuilder : RequiredDialogBuilder<MembershipPackageDialogRequest>
{
    private readonly Func<MembershipPackageDialogViewModel> _createViewModel;

    public MembershipPackageDialogBuilder(Func<MembershipPackageDialogViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.MembershipPackage;

    public override ContentDialog Create(MembershipPackageDialogRequest request)
        => new MembershipPackageDialog(_createViewModel(), request);
}

public sealed class MembershipPackageEditDialogBuilder : RequiredDialogBuilder<MembershipPackageEditDialogRequest>
{
    private readonly Func<MembershipPackageEditDialogViewModel> _createViewModel;

    public MembershipPackageEditDialogBuilder(Func<MembershipPackageEditDialogViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.MembershipPackageEdit;

    public override ContentDialog Create(MembershipPackageEditDialogRequest request)
        => new MembershipPackageEditDialog(_createViewModel(), request);
}

public sealed class GoalKpiDialogBuilder : OptionalDialogBuilder<GoalKpiDialogRequest>
{
    private readonly Func<GoalKpiDialogViewModel> _createViewModel;

    public GoalKpiDialogBuilder(Func<GoalKpiDialogViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.GoalKpi;

    protected override ContentDialog CreateCore(GoalKpiDialogRequest? request)
        => new GoalKpiDialog(_createViewModel(), request);
}

public sealed class TransactionDetailDialogBuilder : RequiredDialogBuilder<TransactionDetailDialogRequest>
{
    private readonly Func<TransactionDetailDialogViewModel> _createViewModel;

    public TransactionDetailDialogBuilder(Func<TransactionDetailDialogViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.TransactionDetail;

    public override ContentDialog Create(TransactionDetailDialogRequest request)
        => new TransactionDetailDialog(_createViewModel(), request);
}

public sealed class TransactionFilterDialogBuilder : OptionalDialogBuilder<TransactionFilterDialogRequest>
{
    private readonly Func<TransactionFilterDialogViewModel> _createViewModel;

    public TransactionFilterDialogBuilder(Func<TransactionFilterDialogViewModel> createViewModel)
    {
        _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
    }

    public override DialogKey Key => DialogKey.TransactionFilter;

    protected override ContentDialog CreateCore(TransactionFilterDialogRequest? request)
        => new TransactionFilterDialog(_createViewModel(), request);
}
