# event-binding
Custom markup extension that allows direct binding of Commands to events, for WPF and Silverlight platforms.

## Usage:

### Namespace reference:

    xmlns:helpers="using:MvvmEventBinding"

### Binding Commands to event handlers:

    <Button Click="{helpers:EventBinding SomeCommand}">click me!</Button>
