using UnityEngine;

public class Library : MonoBehaviour
{
	[SerializeField]
	private LibraryActors m_libraryActors;

	[SerializeField]
	private LibraryClothes m_libraryClothes;

	[SerializeField]
	private LibraryGuns m_libraryGuns;

	[SerializeField]
	private LibraryItems m_libraryItems;

	[SerializeField]
	private LibraryUsables m_libraryUsables;

	private static Library l_instance;

	public static Library Instance
	{
		get
		{
			if (l_instance == null)
			{
				l_instance = Resources.FindObjectsOfTypeAll<Library>()[0];
			}
			return l_instance;
		}
	}

	public LibraryActors Actors => m_libraryActors;

	public LibraryItems Items => m_libraryItems;

	public LibraryGuns Guns => m_libraryGuns;

	public LibraryUsables Usables => m_libraryUsables;

	public LibraryClothes Clothes => m_libraryClothes;
}
