using System;
using Application.Services;
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
    public ConfigDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Config;

    protected override ContentDialog CreateCore()
        => new ConfigDialog(Resolve<ConfigViewModel>());
}

public sealed class RegisterDialogBuilder : ParameterlessDialogBuilder
{
    public RegisterDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Register;

    protected override ContentDialog CreateCore()
        => new RegisterDialog(Resolve<RegisterViewModel>());
}

public sealed class LoginDialogBuilder : ParameterlessDialogBuilder
{
    public LoginDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Login;

    protected override ContentDialog CreateCore()
        => new LoginDialog(Resolve<LoginViewModel>());
}

public sealed class ForgotPasswordDialogBuilder : ParameterlessDialogBuilder
{
    public ForgotPasswordDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.ForgotPassword;

    protected override ContentDialog CreateCore()
        => new ForgotPasswordDialog(Resolve<ForgotPasswordViewModel>());
}

public sealed class OtpDialogBuilder : OptionalDialogBuilder<OtpDialogRequest>
{
    public OtpDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Otp;

    protected override ContentDialog CreateCore(OtpDialogRequest? request)
    {
        var viewModel = Resolve<OtpViewModel>();
        viewModel.Configure(request);
        return new OtpDialog(viewModel);
    }
}

public sealed class ReservationDialogBuilder : OptionalDialogBuilder<ReservationDialogRequest>
{
    public ReservationDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Reservation;

    protected override ContentDialog CreateCore(ReservationDialogRequest? request)
    {
        request ??= new ReservationDialogRequest();
        var viewModel = new ReservationViewModel(
            Resolve<ILocalizationService>(),
            Resolve<ILocalizationPreferencesService>(),
            Resolve<IMemberLookupService>(),
            Resolve<IDialogService>(),
            Resolve<AreaModelFactory>(),
            request.Mode);

        return new ReservationDialog(viewModel, request);
    }
}

public sealed class StartSessionDialogBuilder : RequiredDialogBuilder<StartSessionDialogRequest>
{
    public StartSessionDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.StartSession;

    public override ContentDialog Create(StartSessionDialogRequest request)
        => new StartSessionDialog(Resolve<StartSessionViewModel>(), request.Model);
}

public sealed class AreaFilterDialogBuilder : OptionalDialogBuilder<AreaFilterDialogRequest>
{
    public AreaFilterDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.AreaFilter;

    protected override ContentDialog CreateCore(AreaFilterDialogRequest? request)
        => new AreaFilterDialog(Resolve<AreaFilterViewModel>(), request);
}

public sealed class PaymentDialogBuilder : RequiredDialogBuilder<PaymentDialogRequest>
{
    public PaymentDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Payment;

    public override ContentDialog Create(PaymentDialogRequest request)
        => new PaymentDialog(Resolve<PaymentViewModel>(), request.Model);
}

public sealed class AreaDialogBuilder : OptionalDialogBuilder<AreaDialogRequest>
{
    public AreaDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Area;

    protected override ContentDialog CreateCore(AreaDialogRequest? request)
    {
        var viewModel = new AreaDialogViewModel(
            Resolve<ILocalizationService>(),
            Resolve<IDialogService>(),
            Resolve<AreaModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new AreaDialog(viewModel, request);
    }
}

public sealed class GameFilterDialogBuilder : OptionalDialogBuilder<GameFilterDialogRequest>
{
    public GameFilterDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.GameFilter;

    protected override ContentDialog CreateCore(GameFilterDialogRequest? request)
        => new GameFilterDialog(Resolve<GameFilterViewModel>(), request);
}

public sealed class GameTypeDialogBuilder : RequiredDialogBuilder<GameTypeDialogRequest>
{
    public GameTypeDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.GameType;

    public override ContentDialog Create(GameTypeDialogRequest request)
    {
        var viewModel = Resolve<GameTypeDialogViewModel>();
        viewModel.Configure(request);
        return new GameTypeDialog(viewModel);
    }
}

public sealed class GameDialogBuilder : OptionalDialogBuilder<GameDialogRequest>
{
    public GameDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Game;

    protected override ContentDialog CreateCore(GameDialogRequest? request)
    {
        var viewModel = new GameDialogViewModel(
            Resolve<ILocalizationService>(),
            Resolve<IDialogService>(),
            Resolve<IFilePickerService>(),
            Resolve<GameModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new GameDialog(viewModel, request);
    }
}

public sealed class ProductFilterDialogBuilder : OptionalDialogBuilder<ProductFilterDialogRequest>
{
    public ProductFilterDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.ProductFilter;

    protected override ContentDialog CreateCore(ProductFilterDialogRequest? request)
        => new ProductFilterDialog(Resolve<ProductFilterViewModel>(), request);
}

public sealed class ProductDialogBuilder : OptionalDialogBuilder<ProductDialogRequest>
{
    public ProductDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Product;

    protected override ContentDialog CreateCore(ProductDialogRequest? request)
    {
        var viewModel = new ProductDialogViewModel(
            Resolve<ILocalizationService>(),
            Resolve<IDialogService>(),
            Resolve<IFilePickerService>(),
            Resolve<ProductModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new ProductDialog(viewModel, request);
    }
}

public sealed class MemberDialogBuilder : OptionalDialogBuilder<MemberDialogRequest>
{
    public MemberDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.Member;

    protected override ContentDialog CreateCore(MemberDialogRequest? request)
    {
        var viewModel = new MemberDialogViewModel(
            Resolve<ILocalizationService>(),
            Resolve<IDialogService>(),
            Resolve<MemberModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new MemberDialog(viewModel, request);
    }
}

public sealed class MemberFilterDialogBuilder : OptionalDialogBuilder<MemberFilterDialogRequest>
{
    public MemberFilterDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.MemberFilter;

    protected override ContentDialog CreateCore(MemberFilterDialogRequest? request)
        => new MemberFilterDialog(Resolve<MemberFilterViewModel>(), request);
}

public sealed class MembershipPackageDialogBuilder : RequiredDialogBuilder<MembershipPackageDialogRequest>
{
    public MembershipPackageDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.MembershipPackage;

    public override ContentDialog Create(MembershipPackageDialogRequest request)
        => new MembershipPackageDialog(Resolve<MembershipPackageDialogViewModel>(), request);
}

public sealed class MembershipPackageEditDialogBuilder : RequiredDialogBuilder<MembershipPackageEditDialogRequest>
{
    public MembershipPackageEditDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.MembershipPackageEdit;

    public override ContentDialog Create(MembershipPackageEditDialogRequest request)
        => new MembershipPackageEditDialog(Resolve<MembershipPackageEditDialogViewModel>(), request);
}

public sealed class GoalKpiDialogBuilder : OptionalDialogBuilder<GoalKpiDialogRequest>
{
    public GoalKpiDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.GoalKpi;

    protected override ContentDialog CreateCore(GoalKpiDialogRequest? request)
        => new GoalKpiDialog(Resolve<GoalKpiDialogViewModel>(), request);
}

public sealed class TransactionDetailDialogBuilder : RequiredDialogBuilder<TransactionDetailDialogRequest>
{
    public TransactionDetailDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.TransactionDetail;

    public override ContentDialog Create(TransactionDetailDialogRequest request)
        => new TransactionDetailDialog(Resolve<TransactionDetailDialogViewModel>(), request);
}

public sealed class TransactionFilterDialogBuilder : OptionalDialogBuilder<TransactionFilterDialogRequest>
{
    public TransactionFilterDialogBuilder(IServiceProvider provider)
        : base(provider)
    {
    }

    public override DialogKey Key => DialogKey.TransactionFilter;

    protected override ContentDialog CreateCore(TransactionFilterDialogRequest? request)
        => new TransactionFilterDialog(Resolve<TransactionFilterDialogViewModel>(), request);
}
