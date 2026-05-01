# PlayPointPOS Architecture Conventions

This document keeps the WinUI layer from drifting back into copy-paste, service locator, or direct Infrastructure usage.

## Layer Rules

- `Domain`: entities, enums, and core domain concepts only.
- `Application`: use cases, service contracts, request/response DTOs, filtering/session/rank rules.
- `Infrastructure`: repository and external implementations for Application contracts.
- `WinUI`: presentation state, XAML, UI models, ViewModels, dialog builders, and composition/bootstrap.

Guardrail tests enforce:

- WinUI may reference `Infrastructure.*` only in `WinUI/Composition`.
- Views and UserControls must not call `App.Host`.
- ViewModels must not inject or depend on `IRepository<T>` directly.
- Application must not contain WinUI dialog/navigation presentation contracts.
- `IServiceProvider` and `GetRequiredService` usage must stay in `WinUI/Composition` or app bootstrap.
- Missing product/game images stay empty in state; views render placeholder UI instead of writing mock image data.

## Adding A Page

1. Add a navigation request in `Application/Navigation/Requests`.
2. Add the ViewModel in `WinUI/ViewModels/Pages`.
3. Inject Application services/use cases into the ViewModel. Do not inject repositories.
4. Add the Page XAML and code-behind in `WinUI/Views/Pages`.
5. Keep code-behind UI-only: constructor, `DataContext`, `Loaded`, `SizeChanged`, focus, or responsive measurement.
6. Register the ViewModel and Page in `WinUI/Composition/PlayPointServiceCollectionExtensions.cs`.
7. Register the navigation route in `WinUI/Composition/NavigationRouteRegistry.cs`.

## Adding A Dialog

1. Define a typed request near the dialog ViewModel when the dialog needs input.
2. Add or reuse a ViewModel in `WinUI/ViewModels/Dialogs`.
3. Keep validation and business decisions in Application services when they are not purely UI formatting.
4. Build the XAML with `DialogShell` and reusable dialog controls.
5. Add a dialog builder in `WinUI/Services/Dialogs/DialogBuilders.cs`.
6. Register the builder and any `Func<TViewModel>` factory in `AddDialogBuilders`.
7. Open dialogs through `IDialogService.ShowDialogAsync(...)` or a management dialog coordinator.

Dialog builders should receive their exact dependencies through constructors. Do not inject `IServiceProvider` into a builder.

## Adding A Management Module

1. Put display models in `WinUI/UIModels/Management`.
2. Put business flow rules in `Application`, for example catalog services, filter services, or use-case services.
3. Add UI factories in `WinUI/Services/Factories` for drafts, UI models, and cards.
4. Add a dialog coordinator in `WinUI/Services/Management` for create/edit/delete/dialog notification flows.
5. Use `ManagementQueryState<TFilter, TSort>` for search/filter/sort state.
6. Use `ManagementCollectionController<TItem, TCard>` for pagination, page info, and card cache.
7. Reuse `ManagementPageShell`, `ManagementToolbar`, `SortBar`, `EmptyState`, and pagination controls.
8. Keep the page ViewModel focused on binding state, command wiring, and calling services/controllers.

## Placing Models And Use Cases

- `Application/*`: request/response DTOs, use cases, service contracts, and reusable business rules.
- `WinUI/UIModels/*`: display state shaped for XAML binding. These may implement Application-facing display contracts when needed.
- `WinUI/ViewModels/*`: observable state, commands, and UI orchestration.
- `WinUI/Services/Factories/*`: mapping and draft creation for UI models.
- `WinUI/Services/Management/*`: management dialog and notification coordination.
- `WinUI/Composition/*`: DI registrations, navigation route registration, and Infrastructure wiring.

When a rule is unclear, prefer pushing business decisions toward Application and leaving WinUI with presentation decisions.
