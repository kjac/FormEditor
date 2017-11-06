namespace FormEditor.Fields
{
	public interface IFieldWithValidation
	{
		bool Invalid { get; }

		string ErrorMessage { get; }

		string Name { get; }

		string FormSafeName { get; }
	}
}