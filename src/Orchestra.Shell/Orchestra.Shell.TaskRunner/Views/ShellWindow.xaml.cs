﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellWindow.xaml.cs" company="Orchestra development team">
//   Copyright (c) 2008 - 2014 Orchestra development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orchestra.Views
{
    using System;
    using System.Windows;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.MVVM.Views;
    using Catel.Windows;
    using Logging;
    using Services;

    /// <summary>
    /// Interaction logic for ShellWindow.xaml.
    /// </summary>
    public partial class ShellWindow : IShell
    {
        #region Fields
        private readonly RichTextBoxLogListener _logListener;

        private bool _hasUpdatedViewModel;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellWindow"/> class.
        /// </summary>
        /// <remarks>This method is required for design time support.</remarks>
        public ShellWindow()
            : base(DataWindowMode.Custom, setOwnerAndFocus: false)
        {
            var serviceLocator = this.GetServiceLocator();
            var taskRunnerService = serviceLocator.ResolveType<ITaskRunnerService>();
            var commandManager = serviceLocator.ResolveType<ICommandManager>();

            serviceLocator.RegisterInstance<IAboutInfoService>(taskRunnerService);

            if (taskRunnerService.GetAboutInfo() != null)
            {
                var aboutService = serviceLocator.ResolveType<IAboutService>();
                commandManager.RegisterAction("Help.About", aboutService.ShowAbout);

                AddCustomButton(new DataWindowButton("About", commandManager.GetCommand("Help.About")));
            }

            AddCustomButton(new DataWindowButton("Clear Console", ClearConsole));

            ThemeHelper.EnsureApplicationThemes(GetType().Assembly, true);

            InitializeComponent();

            ConfigurationContext = taskRunnerService.GetViewDataContext();

            var view = taskRunnerService.GetView();

            contentPresenter.Content = view;

            _logListener = new RichTextBoxLogListener(outputTextBox);
            _logListener.IgnoreCatelLogging = true;

            LogManager.AddListener(_logListener);
        }
        #endregion

        #region Properties
        [ViewToViewModel(MappingType = ViewToViewModelMappingType.TwoWayViewWins)]
        public object ConfigurationContext
        {
            get { return (object)GetValue(ConfigurationContextProperty); }
            set { SetValue(ConfigurationContextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConfigurationContext.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigurationContextProperty =
            DependencyProperty.Register("ConfigurationContext", typeof(object), typeof(ShellWindow), new PropertyMetadata(null));
        #endregion

        #region Methods
        protected override void OnViewModelChanged()
        {
            base.OnViewModelChanged();

            // Let the shell create its view model first. Then the view model of the dynamic view model will
            // be created (which means the parent / child view models will work).
            var view = contentPresenter.Content as FrameworkElement;
            if (view != null && !_hasUpdatedViewModel)
            {
                _hasUpdatedViewModel = true;

                ClearConsole();

                view.DataContext = ConfigurationContext;
            }
        }

        private void ClearConsole()
        {
            _logListener.Clear();
        }
        #endregion
    }
}