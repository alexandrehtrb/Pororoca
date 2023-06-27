; ---------------------------------------- 
; This NSIS installer and uninstaller is based on the NSIS_Full demo
; from NsisMultiUser (https://github.com/Drizin/NsisMultiUser)
; this file needs to be saved with UTF-* with BOM encoding
; ---------------------------------------- 


; ---------------------------------------- INSTALLER IMPORTS
!addplugindir /x86-ansi ".\Plugins\x86-ansi"
!addplugindir /x86-unicode ".\Plugins\x86-unicode"
!addincludedir ".\Include"
!addincludedir ".\Common"

!include UAC.nsh
!include NsisMultiUser.nsh
!include LogicLib.nsh
!include StdUtils.nsh
; ---------------------------------------- 

; ---------------------------------------- INSTALLER DEFINES
; uncomment the 3 lines below to test this script using NSIS GUI
; !define INPUT_FILES_DIR "C:\Projetos\Pororoca\out\Pororoca_x.y.z_win-x64_installer" ; TODO: receive this parameter from command-line execution
; !define SHORT_VERSION "x.y.z" ; TODO: receive this parameter from command-line execution
; OutFile "C:\Projetos\Pororoca\out\Pororoca_x.y.z_win-x64_installer\Pororoca_x.y.z_win-x64_installer.exe"
; parameters above shall be received via command-line execution
!define PRODUCT_NAME "Pororoca" ; name of the application as displayed to the user
!define PRODUCT_VERSION "${SHORT_VERSION}.0" ; main version of the application (may be 0.1, alpha, beta, etc.)
!define VERSION "${SHORT_VERSION}.0" ; main version of the application (may be 0.1, alpha, beta, etc.)
!define PROGEXE "Pororoca.exe" ; main application filename
!define COMPANY_NAME "AlexandreHTRB" ; company, used for registry tree hierarchy
; !define CONTACT "" ; stored as the contact information in the uninstall info of the registry
!define COMMENTS "Pororoca desktop executable GUI" ; stored as comments in the uninstall info of the registry
!define URL_INFO_ABOUT "https://github.com/alexandrehtrb/Pororoca" ; stored as the Support Link in the uninstall info of the registry, and when not included, the Help Link as well
; !define URL_HELP_LINK "https://github.com/Drizin/NsisMultiUser/wiki" ; stored as the Help Link in the uninstall info of the registry
; !define URL_UPDATE_INFO "https://github.com/Drizin/NsisMultiUser" ; stored as the Update Information in the uninstall info of the registry
!define PLATFORM "Win64"
!define MIN_WIN_VER "7"
!define SETUP_MUTEX "${COMPANY_NAME} ${PRODUCT_NAME} Setup Mutex" ; do not change this between program versions!
!define APP_MUTEX "${COMPANY_NAME} ${PRODUCT_NAME} App Mutex" ; do not change this between program versions!
!define SETTINGS_REG_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define LICENSE_FILE "LICENCE.md" ; license file, optional

; NsisMultiUser optional defines
!define MULTIUSER_INSTALLMODE_ALLOW_BOTH_INSTALLATIONS 0
!define MULTIUSER_INSTALLMODE_ALLOW_ELEVATION 1
!define MULTIUSER_INSTALLMODE_ALLOW_ELEVATION_IF_SILENT 1 ; required for silent-mode allusers-uninstall to work, when using the workaround for Windows elevation bug
!define MULTIUSER_INSTALLMODE_DEFAULT_ALLUSERS 1
!if ${PLATFORM} == "Win64"
	!define MULTIUSER_INSTALLMODE_64_BIT 1
!endif
!define MULTIUSER_INSTALLMODE_DISPLAYNAME "${PRODUCT_NAME}" ; "${PRODUCT_NAME} ${VERSION} ${PLATFORM}"
; ---------------------------------------- 

; ----------------------------------------  INSTALLER ATTRIBUTES 
Name "${PRODUCT_NAME} ${SHORT_VERSION}"
BrandingText "©2023 ${COMPANY_NAME}"
Icon "${INPUT_FILES_DIR}\pororoca_icon.ico"
VIProductVersion "${PRODUCT_VERSION}"
VIFileVersion "${VERSION}"
VIAddVersionKey "FileVersion" "${VERSION}"
VIAddVersionKey "LegalCopyright" "(C) ${COMPANY_NAME}."
VIAddVersionKey "FileDescription" "Pororoca desktop installer"
AllowSkipFiles off
SetOverwrite on ; (default setting) set to on except for where it is manually switched off
ShowInstDetails show
Unicode true ; properly display all languages (Installer will not work on Windows 95, 98 or ME!)
SetCompressor /SOLID lzma ; TODO: Decide whether to use compression or not
XPStyle on

!include Utils.nsh ; This declaration needs to be at this line
; ---------------------------------------- 

; ---------------------------------------- REMEMBER INSTALLER LANGUAGE
!define LANGDLL_REGISTRY_ROOT SHCTX
!define LANGDLL_REGISTRY_KEY "${SETTINGS_REG_KEY}"
!define LANGDLL_REGISTRY_VALUENAME "Language"
; ---------------------------------------- 

; ---------------------------------------- PAGES
PageEx license
	PageCallbacks PageWelcomeLicensePre EmptyCallback EmptyCallback
	LicenseData $(langLicenseData)
PageExEnd
!insertmacro MULTIUSER_PAGE_INSTALLMODE
PageEx components
	PageCallbacks PageComponentsPre EmptyCallback EmptyCallback
PageExEnd
PageEx directory
	PageCallbacks PageDirectoryPre PageDirectoryShow EmptyCallback
PageExEnd
PageEx instfiles
	PageCallbacks PageInstFilesPre EmptyCallback EmptyCallback
PageExEnd

; Uninstaller pages
ShowUninstDetails show ; Unsintaller attributes
!insertmacro MULTIUSER_UNPAGE_INSTALLMODE
UninstPage components un.PageComponentsPre un.PageComponentsShow un.EmptyCallback
UninstPage instfiles
; ----------------------------------------

; ---------------------------------------- LANGUAGES
; (first alphabetical is default language) - must be inserted after all pages
LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
LoadLanguageFile "${NSISDIR}\Contrib\Language files\PortugueseBR.nlf"
LicenseLangString langLicenseData ${LANG_ENGLISH} "${INPUT_FILES_DIR}\LICENCE.md"
LicenseLangString langLicenseData ${LANG_PORTUGUESEBR} "${INPUT_FILES_DIR}\LICENCE.md"

LangString InstallationTypical ${LANG_ENGLISH} "Typical"
LangString InstallationTypical ${LANG_PORTUGUESEBR} "Típica"

LangString InstallationMinimal ${LANG_ENGLISH} "Minimal"
LangString InstallationMinimal ${LANG_PORTUGUESEBR} "Mínima"

LangString InstallationFull ${LANG_ENGLISH} "Full"
LangString InstallationFull ${LANG_PORTUGUESEBR} "Completa"

LangString SectionProgramFiles ${LANG_ENGLISH} "Program files"
LangString SectionProgramFiles ${LANG_PORTUGUESEBR} "Arquivos do programa"

LangString SectionDocumentation ${LANG_ENGLISH} "Documentation"
LangString SectionDocumentation ${LANG_PORTUGUESEBR} "Documentação"

LangString SectionGroupShortcuts ${LANG_ENGLISH} "Shortcuts"
LangString SectionGroupShortcuts ${LANG_PORTUGUESEBR} "Atalhos" 

LangString SectionStartMenuGroup ${LANG_ENGLISH} "Start Menu group"
LangString SectionStartMenuGroup ${LANG_PORTUGUESEBR} "Grupo no Menu Iniciar"

LangString SectionStartMenuHighlight ${LANG_ENGLISH} "Start Menu highlight"
LangString SectionStartMenuHighlight ${LANG_PORTUGUESEBR} "Destaque no Menu Iniciar"

LangString SectionQuickLaunchShortcut ${LANG_ENGLISH} "Quick Launch icon"
LangString SectionQuickLaunchShortcut ${LANG_PORTUGUESEBR} "Inicialização Rápida"

LangString SectionDesktopShortcut ${LANG_ENGLISH} "Desktop icon"
LangString SectionDesktopShortcut ${LANG_PORTUGUESEBR} "Ícone na Área de Trabalho"

LangString CurrentUserOnly ${LANG_ENGLISH} "(current user only)"
LangString CurrentUserOnly ${LANG_PORTUGUESEBR} "(apenas para o usuário atual)"

LangString ExitSetupQuestion ${LANG_ENGLISH} "Are you sure you want to quit the setup?"
LangString ExitSetupQuestion ${LANG_PORTUGUESEBR} "Tem certeza de que quer sair da instalação?"

LangString ExitUninstallQuestion ${LANG_ENGLISH} "Are you sure you want to quit the uninstall?"
LangString ExitUninstallQuestion ${LANG_PORTUGUESEBR} "Tem certeza de que quer sair da desinstalação?"

LangString CouldNotBeInstalledPleaseRestart ${LANG_ENGLISH} "could not be fully installed.$\r$\nPlease, restart Windows and run the setup program again."
LangString CouldNotBeInstalledPleaseRestart ${LANG_PORTUGUESEBR} "não pôde ser completamente instalado.$\r$\nPor favor, reinicie o Windows e execute o instalador novamente."

LangString CouldNotBeUninstalledPleaseRestart ${LANG_ENGLISH} "could not be fully uninstalled.$\r$\nPlease, restart Windows and run the uninstaller again."
LangString CouldNotBeUninstalledPleaseRestart ${LANG_PORTUGUESEBR} "não pôde ser completamente desinstalado.$\r$\nPor favor, reinicie o Windows e execute o desisntalador novamente."

LangString UserCollectionsAndPreferencesWereNotDeleted ${LANG_ENGLISH} "Your collections and preferences will not be deleted. They are located at: 'Users\{you}\AppData\Roaming\Pororoca\PororocaUserData\'."
LangString UserCollectionsAndPreferencesWereNotDeleted ${LANG_PORTUGUESEBR} "Suas coleções e preferências não serão excluídas. Elas estão em: 'Users\{você}\AppData\Roaming\Pororoca\PororocaUserData\'."

LangString InstallationCompleted ${LANG_ENGLISH} "Installation successfully completed!"
LangString InstallationCompleted ${LANG_PORTUGUESEBR} "Instalação concluída com sucesso!"
CompletedText $(InstallationCompleted)


!insertmacro MULTIUSER_LANGUAGE_INIT

!macro LANGDLL_DISPLAY
	${ifnot} ${silent}
		ReadRegStr $LANGUAGE ${LANGDLL_REGISTRY_ROOT} "${LANGDLL_REGISTRY_KEY}" "${LANGDLL_REGISTRY_VALUENAME}"
		${if} "$LANGUAGE" == ""
		    ; languages will be alphabetically sorted, first alpabetical will be selected
			Push ""
			Push ${LANG_ENGLISH}
			Push "English"
			Push ${LANG_PORTUGUESEBR}
			Push "Português"
			Push "A" ; A means auto count languages; for the auto count to work the first empty push (Push "") must remain
			LangDLL::LangDialog "Installer Language" "Please select the language of the installer"

			Pop $LANGUAGE
			${if} "$LANGUAGE" == "cancel"
				Abort
			${endif}
		${endif}
	${endif}
!macroend

; Reserve files
ReserveFile /plugin LangDLL.dll
; ----------------------------------------

; ---------------------------------------- MULTIUSER HELPER FUNCTIONS
Function CheckInstallation 
	; if there's an installed version, uninstall it first (I chose not to start the uninstaller silently, so that user sees what failed)
	; if both per-user and per-machine versions are installed, unistall the one that matches $MultiUser.InstallMode
	StrCpy $0 ""
	${if} $HasCurrentModeInstallation = 1
		StrCpy $0 "$MultiUser.InstallMode"
	${else}
		!if ${MULTIUSER_INSTALLMODE_ALLOW_BOTH_INSTALLATIONS} = 0
			${if} $HasPerMachineInstallation = 1
				StrCpy $0 "AllUsers" ; if there's no per-user installation, but there's per-machine installation, uninstall it
			${elseif} $HasPerUserInstallation = 1
				StrCpy $0 "CurrentUser" ; if there's no per-machine installation, but there's per-user installation, uninstall it
			${endif}
		!endif
	${endif}

	${if} "$0" != ""
		${if} $0 == "AllUsers"
			StrCpy $1 "$PerMachineUninstallString"
			StrCpy $3 "$PerMachineInstallationFolder"
		${else}
			StrCpy $1 "$PerUserUninstallString"
			StrCpy $3 "$PerUserInstallationFolder"
		${endif}
		${if} ${silent}
			StrCpy $2 "/S"
		${else}
			StrCpy $2 ""
		${endif}
	${endif}
FunctionEnd

Function RunUninstaller
	StrCpy $0 0
	ExecWait '$1 /SS $2 _?=$3' $0 ; $1 is quoted in registry; the _? param stops the uninstaller from copying itself to the temporary directory, which is the only way for ExecWait to work
FunctionEnd
; ---------------------------------------- 

; ---------------------------------------- SECTIONS
InstType $(InstallationTypical)
InstType $(InstallationMinimal)
InstType $(InstallationFull)

Section !$(SectionProgramFiles) SectionCoreFiles
	SectionIn 1 2 3 RO

	!insertmacro UAC_AsUser_Call Function CheckInstallation ${UAC_SYNCREGISTERS}
	${if} $0 != ""
		HideWindow
		ClearErrors
		${if} $0 == "AllUsers"
			Call RunUninstaller
  		${else}
			!insertmacro UAC_AsUser_Call Function RunUninstaller ${UAC_SYNCREGISTERS}
  		${endif}
		${if} ${errors} ; stay in installer
			SetErrorLevel 2 ; Installation aborted by script
			BringToFront
			Abort "Error executing uninstaller."
		${else}
			${Switch} $0
				${Case} 0 ; uninstaller completed successfully - continue with installation
					BringToFront
					Sleep 1000 ; wait for cmd.exe (called by the uninstaller) to finish
					${Break}
				${Case} 1 ; Installation aborted by user (cancel button)
				${Case} 2 ; Installation aborted by script
					SetErrorLevel $0
					Quit ; uninstaller was started, but completed with errors - Quit installer
				${Default} ; all other error codes - uninstaller could not start, elevate, etc. - Abort installer
					SetErrorLevel $0
					BringToFront
					Abort "Error executing uninstaller."
			${EndSwitch}
		${endif}

		; Just a failsafe - should've been taken care of by cmd.exe
		!insertmacro DeleteRetryAbort "$3\${UNINSTALL_FILENAME}" ; the uninstaller doesn't delete itself when not copied to the temp directory
		RMDir "$3"
	${endif}

	SetOutPath $INSTDIR
	; Write uninstaller and registry uninstall info as the first step,
	; so that the user has the option to run the uninstaller if sth. goes wrong
	WriteUninstaller "${UNINSTALL_FILENAME}"
	; or this if you're using signing:
	; File "${UNINSTALL_FILENAME}"
	!insertmacro MULTIUSER_RegistryAddInstallInfo ; add registry keys
	WriteRegStr "${LANGDLL_REGISTRY_ROOT}" "${LANGDLL_REGISTRY_KEY}" "${LANGDLL_REGISTRY_VALUENAME}" $LANGUAGE ; write language

	File /r "${INPUT_FILES_DIR}\*.*"
	
SectionEnd

; TODO: Add documentation as an optional installation feature
; Section /o $(SectionDocumentation) SectionDocumentation
; 	SectionIn 3
; 
; 	SetOutPath $INSTDIR
; 	File "${INPUT_FILES_DIR}\LICENCE.md"
; SectionEnd

Section $(SectionStartMenuGroup) SectionProgramGroup
	SectionIn 1 2 3 RO

	!insertmacro MULTIUSER_GetCurrentUserString $0
	CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}$0"
	CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}$0\${PRODUCT_NAME}.lnk" "$INSTDIR\${PROGEXE}"

	!ifdef LICENSE_FILE
		CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}$0\License Agreement.lnk" "$INSTDIR\${LICENSE_FILE}"
	!endif
	${if} $MultiUser.InstallMode == "AllUsers"
		CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}$0\Uninstall.lnk" "$INSTDIR\${UNINSTALL_FILENAME}" "/allusers"
	${else}
		CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}$0\Uninstall.lnk" "$INSTDIR\${UNINSTALL_FILENAME}" "/currentuser"
	${endif}
SectionEnd

SectionGroup /e $(SectionGroupShortcuts) SectionGroupIntegration

Section $(SectionDesktopShortcut) SectionDesktopIcon
	SectionIn 1 3

	!insertmacro MULTIUSER_GetCurrentUserString $0
	CreateShortCut "$DESKTOP\${PRODUCT_NAME}$0.lnk" "$INSTDIR\${PROGEXE}"
SectionEnd

Section /o $(SectionStartMenuHighlight) SectionStartMenuIcon
	SectionIn 3

	; this section fixates the shortcut in Start Menu's highlights
	${if} ${AtLeastWin7}
		${StdUtils.InvokeShellVerb} $0 "$INSTDIR" "${PROGEXE}" ${StdUtils.Const.ShellVerb.PinToStart}
	${else}
		!insertmacro MULTIUSER_GetCurrentUserString $0
		CreateShortCut "$STARTMENU\${PRODUCT_NAME}$0.lnk" "$INSTDIR\${PROGEXE}"
	${endif}
SectionEnd

Section /o $(SectionQuickLaunchShortcut) SectionQuickLaunchIcon
	SectionIn 3

	${if} ${AtLeastWin7}
		${StdUtils.InvokeShellVerb} $0 "$INSTDIR" "${PROGEXE}" ${StdUtils.Const.ShellVerb.PinToTaskbar}
	${else}
		; The $QUICKLAUNCH folder is always only for the current user
		CreateShortCut "$QUICKLAUNCH\${PRODUCT_NAME}.lnk" "$INSTDIR\${PROGEXE}"
	${endif}
SectionEnd
SectionGroupEnd

Section "-Write Install Size" ; hidden section, write install size as the final step
	!insertmacro MULTIUSER_RegistryAddInstallSizeInfo
SectionEnd
; ----------------------------------------

; ---------------------------------------- PAGES CALLBACKS
Function EmptyCallback
FunctionEnd

Function PageWelcomeLicensePre
	${if} $InstallShowPagesBeforeComponents = 0
		Abort ; don't display the Welcome and License pages
	${endif}
FunctionEnd

Function PageComponentsPre
	GetDlgItem $0 $HWNDPARENT 1
	SendMessage $0 ${BCM_SETSHIELD} 0 0 ; hide SHIELD (Windows Vista and above)

	${if} $MultiUser.InstallMode == "AllUsers"
		; TODO: This section seems to make no difference, should it be removed?
		${if} ${AtLeastWin7} ; add "(current user only)" text to section "Start Menu Icon"
			SectionGetText ${SectionStartMenuIcon} $0
			SectionSetText ${SectionStartMenuIcon} "$0 $(CurrentUserOnly)"
		${endif}

		; add "(current user only)" text to section "Quick Launch Icon"
		SectionGetText ${SectionQuickLaunchIcon} $0
		SectionSetText ${SectionQuickLaunchIcon} "$0 $(CurrentUserOnly)"
	${endif}
FunctionEnd

Function PageDirectoryPre
	GetDlgItem $1 $HWNDPARENT 1
	SendMessage $1 ${WM_SETTEXT} 0 "STR:$(^InstallBtn)" ; this is the last page before installing
	Call MultiUser.CheckPageElevationRequired
	${if} $0 = 2
		SendMessage $1 ${BCM_SETSHIELD} 0 1 ; display SHIELD (Windows Vista and above)
	${endif}
FunctionEnd

Function PageDirectoryShow
	${if} $CmdLineDir != ""
		FindWindow $R1 "#32770" "" $HWNDPARENT

		GetDlgItem $0 $R1 1019 ; Directory edit
		SendMessage $0 ${EM_SETREADONLY} 1 0 ; read-only is better than disabled, as user can copy contents

		GetDlgItem $0 $R1 1001 ; Browse button
		EnableWindow $0 0
	${endif}
FunctionEnd

Function PageInstFilesPre
	GetDlgItem $0 $HWNDPARENT 1
	SendMessage $0 ${BCM_SETSHIELD} 0 0 ; hide SHIELD (Windows Vista and above)
FunctionEnd
; ---------------------------------------- 

; ---------------------------------------- ON INIT / ON USER CALLBACKS
Function .onInit
	!insertmacro CheckPlatform ${PLATFORM}
	!insertmacro CheckMinWinVer ${MIN_WIN_VER}
	${ifnot} ${UAC_IsInnerInstance}
		!insertmacro CheckSingleInstance "Setup" "Global" "${SETUP_MUTEX}"
		!insertmacro CheckSingleInstance "Application" "Local" "${APP_MUTEX}"
	${endif}

	!insertmacro MULTIUSER_INIT
	
	${if} $IsInnerInstance = 0
		!insertmacro LANGDLL_DISPLAY
	${endif}
FunctionEnd

Function .onUserAbort
	MessageBox MB_YESNO|MB_ICONEXCLAMATION $(ExitSetupQuestion) IDYES mui.quit

	Abort
	mui.quit:
FunctionEnd

Function .onInstFailed
	MessageBox MB_ICONSTOP "${PRODUCT_NAME} ${VERSION} $(CouldNotBeInstalledPleaseRestart)" /SD IDOK
FunctionEnd
; ----------------------------------------

; ---------------------------------------- UNINSTALLER INCLUDE
; remove next line if you're using signing after the uninstaller is extracted from the initially compiled setup
!include Uninstaller.nsh
