# Common.ComponentBase
Common.ComponentBase provides component classes and helpers for .Net
As examples:
- class AuthenticationService provides helper methods for authenticating with MSAL
- class DebugHelper provides helper methods for troubleshooting support
- class SerializationHelper provides helper methods for serialization
- ...

# Common.PresentationBase
Common.PresentationBase provides helper classes for Windows Presentation Foundation applications
In particular it provides:
- __WindowBase__ class: a base class for windows to support resizable windows with no chrome.
  see _WindowBaseSample_ as an example for use of WindowBase. 
- __ApplicationWindow__ class: base application window designed to contain UserControls
  UserControls can be added and removed easily to an an ApplicationWindow by means of commands AddItem and RemoveItem.
  Controls are hosted by the ApplicationWindow into the Items collection.
  ApplicationWindow is loosely coupled from its content that can be added and removed easily during the application lifetime.
  see _ApplicationWindow_ as an example for use of ApplicationWindow. 
- __Common conversions__ for xaml bindings: a set of XAML converters that implement common operations on boolean, strings, numbers:<br>
  provided converters include:<br>
  booleanToVisibility, booleanToVisibilityHidden, booleanToString<br>
  format, formatNN, formatDate<br>
  isNull, isNotNull<br>
  isEqual, isNotEqual<br>
  trim, trimEnd, trimStart<br>
  is, isNot, hasFlag<br>
  if, nullCoalesce<br>
  ifInDesignMode, ifNotInDesignMode<br>
  isNullOrEmpty, isNotNullOrEmpty, isNullOrWhiteSpace, isNotNullOrWhiteSpace<br>
  toUpper, toLower<br>
  lt, lte, gt, gte<br>
  and, or, nand, nor, xor xnor, not<br>
  rnd, rndDouble<br>
  isIn, contains, doesNotContain<br>
  replace<br>
  selectItem<br>
  isCollectionNullOrEmpty, isCollectionNotNullOrEmpty<br>
  add, sub, mult, div<br>
  zeroToNull, zeroShortToNull, zeroIntToNull, zeroLongToNull, zeroFloatToNull, zeroDoubleToNull, zeroStringToNull, zeroDecimalToNull, zeroIntToDash<br>
  minDateToNull, maxDateToNull<br>
  nullToUnsetValue, minDateToUnsetValue, zeroToUnsetValue, zeroShortToUnsetValue, zeroIntToUnsetValue, zeroLongToUnsetValue, zeroFloatToUnsetValue, zeroDoubleToUnsetValue, zeroStringToUnsetValue, zeroDecimalToUnsetValue<br>
  addDays, addWorkingDays<br>
  getType,<br>
  EventConverter, DelegateConverter to provide conversions based on events and deegates<br>
  and many others<br>
  
- __Attached properties__ for xaml bindings:
  data can be combined with Xaml bindings and multibindings by means of conversions and attached properties.
  AttachedProperties class provides a convinient set of attached properties that can be used for that:
  IsVisible0, IsVisible1, IsVisible3...<br>
  Boolean0, Boolean1, Boolean3...<br>
  String0, String1, String3...<br>
  Integer0, Integer1, Integer3...<br>
  Exception0, Exception1, Exception3...<br>
  ...<br>
  
  the example below shows how to use attached properties and conversions to manage visibility of items based on data on data on the DataContext (eg. the ViewModel):<br>
  
```c#
  Visibility="{Binding RelativeSource={RelativeSource Self}, Path=(common:AttachedProperties.IsVisible0), Mode=OneWay, Converter={StaticResource booleanToVisibility}}"
  common:AttachedProperties.IsVisible0="{Binding Path=DocumentPath, Mode=OneWay, Converter={StaticResource isNotNullOrEmpty}, FallbackValue=false}"
```
- __Localization Support__ for xaml bindings and c# code: 
  localize conversion, ToLocalize() extension method allow quick translation of text and constants, according to the current UI culture
  Uid Attached Property allow to apply localization to the result of bindings and set it to control properties

- __Exception Management and data Validation__ for xaml: default UI is provided for unhandled exceptions and validators are provided to apply validation to all kind of data


- __Other helper classes__ for client applications: helper classes are provided for working with client applications.


# Common.EntityBase
Common.EntityBase provides .Net Standard entity base with INotifyProperyChanged and ISupportKey implementation
EntityBase can be used as a base class for all entities to make sure that all your application data supports these interfaces. 

INotifyProperyChanged support is provided backing properties content on a Dictionary to be  more space efficient.
Properties with PropertyChanged notification may be implemented with the following syntax, with _little redundance_ and __compile time generation of the name triggered with the notification__:


        #region ClientId
        public string ClientId
        {
            get { return GetValue(() => ClientId); }
            set { SetValue(() => ClientId, value); }
        }
        #endregion

# Common.AssemblyResolver
AssemblyResolver provides resolution of assembly binding redirection specified in custom XML or resource files 








