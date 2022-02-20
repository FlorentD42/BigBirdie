namespace BigBirdie.Hubs
{
	public interface IQuizHub
	{
		/// <summary>
		/// envoie les informations de session au client
		/// </summary>
		/// <param name="sessionJson"></param>
		/// <returns></returns>
		Task SessionUpdate(string? sessionJson);

		/// <summary>
		/// Affiche un message d’erreur à l’utilisateur, redirection côté client
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		Task Error(string message);

		/// <summary>
		/// Informe à l’utilisateur qu’il est le créateur de ce salon
		/// </summary>
		Task IsOwner();
	}
}
