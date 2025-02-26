using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Services
{
	/// <summary>
	/// Manages the storage and retrieval of AI answers
	/// </summary>
	public class AnswerStoreManager
	{
		// Singleton pattern implementation
		private static readonly Lazy<AnswerStoreManager> _instance =
			new Lazy<AnswerStoreManager>(() => new AnswerStoreManager());

		public static AnswerStoreManager Instance => _instance.Value;

		// Thread-safe storage for answers
		private readonly ConcurrentDictionary<string, object> _answerStore = new ConcurrentDictionary<string, object>();

		// Private constructor for singleton
		private AnswerStoreManager() { }

		/// <summary>
		/// Stores an answer with the given ID
		/// </summary>
		/// <param name="id">The unique identifier for the answer</param>
		/// <param name="answer">The answer object to store</param>
		/// <returns>True if the answer was stored successfully</returns>
		public bool StoreAnswer(string id, object answer)
		{
			return _answerStore.TryAdd(id, answer);
		}

		/// <summary>
		/// Retrieves an answer by its ID
		/// </summary>
		/// <param name="id">The unique identifier of the answer</param>
		/// <param name="answer">The retrieved answer (out parameter)</param>
		/// <returns>True if the answer was found</returns>
		public bool TryGetAnswer(string id, out object answer)
		{
			return _answerStore.TryGetValue(id, out answer);
		}

		/// <summary>
		/// Updates an existing answer
		/// </summary>
		/// <param name="id">The unique identifier of the answer</param>
		/// <param name="answer">The updated answer object</param>
		/// <returns>True if the answer was updated successfully</returns>
		public bool UpdateAnswer(string id, object answer)
		{
			return _answerStore.TryUpdate(id, answer, _answerStore[id]);
		}

		/// <summary>
		/// Removes an answer from the store
		/// </summary>
		/// <param name="id">The unique identifier of the answer to remove</param>
		/// <returns>True if the answer was removed successfully</returns>
		public bool RemoveAnswer(string id)
		{
			return _answerStore.TryRemove(id, out _);
		}

		/// <summary>
		/// Gets all stored answer IDs
		/// </summary>
		/// <returns>A list of all answer IDs in the store</returns>
		public List<string> GetAllAnswerIds()
		{
			return new List<string>(_answerStore.Keys);
		}

		/// <summary>
		/// Clears all stored answers
		/// </summary>
		public void ClearAllAnswers()
		{
			_answerStore.Clear();
		}
	}
}