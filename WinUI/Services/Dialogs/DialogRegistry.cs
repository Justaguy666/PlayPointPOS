using System;
using Application.Services;
using Application.Services.Members;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinUI.Services.Factories;
using WinUI.UIModels.Enums;
using WinUI.UIModels.Management;
using WinUI.ViewModels.Dialogs;
using WinUI.ViewModels.Dialogs.Dashboard;
using WinUI.ViewModels.Dialogs.Management;
using WinUI.Views.Dialogs;
using WinUI.Views.Dialogs.Dashboard;
using WinUI.Views.Dialogs.Management;

namespace WinUI.Services.Dialogs;

public sealed class DialogRegistry : IDialogFactory
{
    private readonly IServiceProvider _provider;

    public DialogRegistry(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public ContentDialog? Create(DialogKey dialogKey, object? parameter)
    {
        return dialogKey switch
        {
            DialogKey.Config => new ConfigDialog(_provider.GetRequiredService<ConfigViewModel>()),
            DialogKey.Register => new RegisterDialog(_provider.GetRequiredService<RegisterViewModel>()),
            DialogKey.Login => new LoginDialog(_provider.GetRequiredService<LoginViewModel>()),
            DialogKey.ForgotPassword => new ForgotPasswordDialog(_provider.GetRequiredService<ForgotPasswordViewModel>()),
            DialogKey.Otp => CreateOtpDialog(parameter),
            DialogKey.Reservation => CreateReservationDialog(parameter),
            DialogKey.AreaFilter => CreateAreaFilterDialog(parameter),
            DialogKey.GameFilter => CreateGameFilterDialog(parameter),
            DialogKey.Payment => CreatePaymentDialog(parameter),
            DialogKey.StartSession => CreateStartSessionDialog(parameter),
            DialogKey.Area => CreateAreaDialog(parameter),
            DialogKey.Game => CreateGameDialog(parameter),
            DialogKey.Product => CreateProductDialog(parameter),
            DialogKey.ProductFilter => CreateProductFilterDialog(parameter),
            DialogKey.Member => CreateMemberDialog(parameter),
            DialogKey.MemberFilter => CreateMemberFilterDialog(parameter),
            DialogKey.MembershipPackage => CreateMembershipPackageDialog(parameter),
            DialogKey.MembershipPackageEdit => CreateMembershipPackageEditDialog(parameter),
            DialogKey.GameType => CreateGameTypeDialog(parameter),
            DialogKey.GoalKpi => CreateGoalKpiDialog(parameter),
            DialogKey.TransactionDetail => CreateTransactionDetailDialog(parameter),
            DialogKey.TransactionFilter => CreateTransactionFilterDialog(parameter),
            _ => null,
        };
    }

    private ContentDialog CreateOtpDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<OtpViewModel>();
        viewModel.Configure(parameter as OtpDialogRequest);
        return new OtpDialog(viewModel);
    }

    private ContentDialog CreateReservationDialog(object? parameter)
    {
        var request = parameter switch
        {
            ReservationDialogRequest reservationRequest => reservationRequest,
            AreaModel areaModel => new ReservationDialogRequest
            {
                Mode = UpsertDialogMode.Add,
                Model = areaModel,
            },
            _ => new ReservationDialogRequest(),
        };

        var viewModel = new ReservationViewModel(
            _provider.GetRequiredService<ILocalizationService>(),
            _provider.GetRequiredService<ILocalizationPreferencesService>(),
            _provider.GetRequiredService<IMemberLookupService>(),
            _provider.GetRequiredService<IDialogService>(),
            _provider.GetRequiredService<AreaModelFactory>(),
            request.Mode);

        return new ReservationDialog(viewModel, request);
    }

    private ContentDialog CreateStartSessionDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<StartSessionViewModel>();
        return new StartSessionDialog(viewModel, parameter as AreaModel);
    }

    private ContentDialog CreateAreaFilterDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<AreaFilterViewModel>();
        return new AreaFilterDialog(viewModel, parameter as AreaFilterDialogRequest);
    }

    private ContentDialog CreatePaymentDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<PaymentViewModel>();
        return new PaymentDialog(viewModel, parameter as AreaModel);
    }

    private ContentDialog CreateAreaDialog(object? parameter)
    {
        var request = parameter as AreaDialogRequest;
        var viewModel = new AreaDialogViewModel(
            _provider.GetRequiredService<ILocalizationService>(),
            _provider.GetRequiredService<IDialogService>(),
            _provider.GetRequiredService<AreaModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new AreaDialog(viewModel, request);
    }

    private ContentDialog CreateGameFilterDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<GameFilterViewModel>();
        return new GameFilterDialog(viewModel, parameter as GameFilterDialogRequest);
    }

    private ContentDialog CreateGameTypeDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<GameTypeDialogViewModel>();
        if (parameter is GameTypeDialogRequest request)
        {
            viewModel.Configure(request);
        }

        return new GameTypeDialog(viewModel);
    }

    private ContentDialog CreateGameDialog(object? parameter)
    {
        var request = parameter as GameDialogRequest;
        var viewModel = new GameDialogViewModel(
            _provider.GetRequiredService<ILocalizationService>(),
            _provider.GetRequiredService<IDialogService>(),
            _provider.GetRequiredService<GameModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new GameDialog(
            viewModel,
            request,
            _provider.GetRequiredService<MainWindow>());
    }

    private ContentDialog CreateProductFilterDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<ProductFilterViewModel>();
        return new ProductFilterDialog(viewModel, parameter as ProductFilterDialogRequest);
    }

    private ContentDialog CreateTransactionDetailDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<TransactionDetailDialogViewModel>();
        return new TransactionDetailDialog(viewModel, parameter as TransactionDetailDialogRequest);
    }

    private ContentDialog CreateTransactionFilterDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<TransactionFilterDialogViewModel>();
        return new TransactionFilterDialog(viewModel, parameter as TransactionFilterDialogRequest);
    }

    private ContentDialog CreateProductDialog(object? parameter)
    {
        var request = parameter as ProductDialogRequest;
        var viewModel = new ProductDialogViewModel(
            _provider.GetRequiredService<ILocalizationService>(),
            _provider.GetRequiredService<IDialogService>(),
            _provider.GetRequiredService<ProductModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new ProductDialog(
            viewModel,
            request,
            _provider.GetRequiredService<MainWindow>());
    }

    private ContentDialog CreateMemberDialog(object? parameter)
    {
        var request = parameter as MemberDialogRequest;
        var viewModel = new MemberDialogViewModel(
            _provider.GetRequiredService<ILocalizationService>(),
            _provider.GetRequiredService<IDialogService>(),
            _provider.GetRequiredService<MemberModelFactory>(),
            request?.Mode ?? UpsertDialogMode.Add);

        return new MemberDialog(viewModel, request);
    }

    private ContentDialog CreateMemberFilterDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<MemberFilterViewModel>();
        return new MemberFilterDialog(viewModel, parameter as MemberFilterDialogRequest);
    }

    private ContentDialog CreateMembershipPackageDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<MembershipPackageDialogViewModel>();
        return new MembershipPackageDialog(
            viewModel,
            parameter as MembershipPackageDialogRequest
                ?? throw new InvalidOperationException("Membership package dialog request is required."));
    }

    private ContentDialog CreateMembershipPackageEditDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<MembershipPackageEditDialogViewModel>();
        return new MembershipPackageEditDialog(
            viewModel,
            parameter as MembershipPackageEditDialogRequest
                ?? throw new InvalidOperationException("Membership package edit dialog request is required."));
    }

    private ContentDialog CreateGoalKpiDialog(object? parameter)
    {
        var viewModel = _provider.GetRequiredService<GoalKpiDialogViewModel>();
        return new GoalKpiDialog(viewModel, parameter as GoalKpiDialogRequest);
    }
}
