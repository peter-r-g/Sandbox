using System.Collections.Generic;
using Sandbox;

namespace SandboxGame;

public static class Language
{
	private static string _backupLanguageName;

	private static Dictionary<string, string> _primaryLanguage;
	private static Dictionary<string, string> _backupLanguage;
	public static string PrimaryLanguage => ConsoleSystem.GetValue( "cl_language" );

	public static string BackupLanguage
	{
		get => _backupLanguageName;
		set
		{
			_backupLanguageName = value;
			_backupLanguage = FileSystem.Mounted.ReadJson<Dictionary<string, string>>( $"lang/{value}.json" );
		}
	}

	public static void Initialize( string backupLang )
	{
		_primaryLanguage = FileSystem.Mounted.ReadJson<Dictionary<string, string>>(
			$"lang/{PrimaryLanguage}.json" );
		BackupLanguage = backupLang;
	}

	public static string GetPhrase( string phrase )
	{
		if ( _primaryLanguage.TryGetValue( phrase, out var text ) )
		{
			return text;
		}

		return _backupLanguage.TryGetValue( phrase, out text ) ? text : null;
	}

	public static string GetPhrase<T>( string phrase, T arg1 )
	{
		if ( _primaryLanguage.TryGetValue( phrase, out var text ) )
		{
			return string.Format( text, arg1 );
		}

		return _backupLanguage.TryGetValue( phrase, out text ) ? string.Format( text, arg1 ) : null;
	}

	public static string GetPhrase<T1, T2>( string phrase, T1 arg1, T2 arg2 )
	{
		if ( _primaryLanguage.TryGetValue( phrase, out var text ) )
		{
			return string.Format( text, arg1, arg2 );
		}

		return _backupLanguage.TryGetValue( phrase, out text ) ? string.Format( text, arg1, arg2 ) : null;
	}

	public static string TryGetPhrase( string phrase, bool poundIncluded = true )
	{
		var phraseText = GetPhrase( phrase );
		if ( phraseText is not null )
		{
			return phraseText;
		}

		if ( poundIncluded && !phrase.StartsWith( '#' ) )
		{
			return phrase;
		}

		phraseText = GetPhrase( phrase[1..] );
		return phraseText ?? phrase;
	}

	public static string TryGetPhrase<T>( string phrase, T arg1, bool poundIncluded = true )
	{
		var phraseText = GetPhrase( phrase, arg1 );
		if ( phraseText is not null )
		{
			return phraseText;
		}

		if ( poundIncluded && !phrase.StartsWith( '#' ) )
		{
			return phrase;
		}

		phraseText = GetPhrase( phrase[1..], arg1 );
		return phraseText ?? phrase;
	}

	public static string TryGetPhrase<T1, T2>( string phrase, T1 arg1, T2 arg2, bool poundIncluded = true )
	{
		var phraseText = GetPhrase( phrase, arg1, arg2 );
		if ( phraseText is not null )
		{
			return phraseText;
		}

		if ( poundIncluded && !phrase.StartsWith( '#' ) )
		{
			return phrase;
		}

		phraseText = GetPhrase( phrase[1..], arg1, arg2 );
		return phraseText ?? phrase;
	}
}
