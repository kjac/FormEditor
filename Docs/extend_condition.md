# Creating a custom condition
Conditions are used for both actions and cross field validations:
* Actions let the editors set up rules that are evaluated against the field values - e.g. to show/hide a field if another field has a certain value. 
* Cross field validations let the editors invalidate the form based on a set of rules that span across multiple fields. 

In both cases, a rule consists of:
* A field.
* A condition to match the field value against.

You can create custom conditions in one of two ways:

1. [The simple way](extend_condition_simple.md) - by configuration. No Visual Studio required. Choose this if you're not a .NET developer, or if you don't need server side support for your condition.
2. [The advanced way](extend_condition_advanced.md) - by coding. Visual Studio required. Choose this if you need server side support - which is of course strongly recommended if you're going to use your condition for cross field validations.
