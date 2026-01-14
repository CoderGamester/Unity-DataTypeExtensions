namespace GameLovers.Observables
{
	public enum ObservableUpdateType
	{
		Added,
		Updated,
		Removed
	}

	public enum ObservableUpdateFlag
	{
		// Updates all subsribers that didn't specify the key index
		UpdateOnly,
		// Updates only for subscripers that added their key index
		KeyUpdateOnly,
		// Updates all types of subscribers [This has a high performance cost]
		Both
	}
}
