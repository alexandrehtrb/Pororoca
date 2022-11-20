!include LogicLib.nsh
!include x64.nsh

!define ERROR_ALREADY_EXISTS 0x000000b7
!define ERROR_ACCESS_DENIED 0x5

!macro CheckPlatform PLATFORM
	${if} ${RunningX64}
		!if ${PLATFORM} == "Win32"
			MessageBox MB_ICONSTOP "Please, run the 64-bit installer of ${PRODUCT_NAME} on this version of Windows." /SD IDOK
			Quit ; will SetErrorLevel 2 - Installation aborted by script
		!endif
	${else}
		!if ${PLATFORM} == "Win64"
			MessageBox MB_ICONSTOP "Please, run the 32-bit installer of ${PRODUCT_NAME} on this version of Windows." /SD IDOK
			Quit ; will SetErrorLevel 2 - Installation aborted by script
		!endif
	${endif}		
!macroend

!macro CheckMinWinVer MIN_WIN_VER
	${ifnot} ${AtLeastWin${MIN_WIN_VER}}	
		MessageBox MB_ICONSTOP "This program requires at least Windows ${MIN_WIN_VER}." /SD IDOK
		Quit ; will SetErrorLevel 2 - Installation aborted by script
	${endif}	
!macroend

!macro CheckSingleInstanceFunc UNINSTALLER_PREFIX
	; parameters:
	; $0 - TYPE - "Setup" or "Application"
	; $1 - SCOPE - "Global" or "Local" (default if empty)
	; $2 - MUTEX_NAME - unique mutex name
	Function ${UNINSTALLER_PREFIX}CheckSingleInstance
		Push $3
		Push $4
		Push $5

		${if} $1 == ""
			StrCpy $1 "Local"
		${endif}

		${if} $0 == "Setup"
			StrCpy $5 "The setup of ${PRODUCT_NAME}"
		${else}
			StrCpy $5 "${PRODUCT_NAME}"
		${endif}

		try:
		System::Call 'kernel32::CreateMutex(i 0, i 0, t "$1\$2") i .r3 ?e'
		Pop $4 ; the stack contains the result of GetLastError

		${if} $0 == "Application"
		    ${andif} $3 <> 0
			System::Call 'kernel32::CloseHandle(i $3)' ; close the Application mutex
		${endif}

		${if} $4 = ${ERROR_ALREADY_EXISTS}
			${orif} $4 = ${ERROR_ACCESS_DENIED}	; ERROR_ACCESS_DENIED means the mutex was created by another user and we don't have access to open it, so application is running
			; will display NSIS taskbar button, no way to hide it before GUIInit, $HWNDPARENT is 0
			MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION "$5 is already running.$\r$\n\
				Please, close all instances of it and click Retry to continue, or Cancel to exit." /SD IDCANCEL IDCANCEL cancel
			System::Call 'kernel32::CloseHandle(i $3)' ; for next CreateMutex call to succeed
			Goto try

			cancel:
			Quit ; will SetErrorLevel 2 - Installation aborted by script
		${endif}

		Pop $5
		Pop $4
		Pop $3
	FunctionEnd
!macroend

!macro DeleteRetryAbortFunc UNINSTALLER_PREFIX
	; parameters:
	; $0 - FILE_NAME - file to delete
	Function ${UNINSTALLER_PREFIX}DeleteRetryAbort
		; unlike the File instruction, Delete doesn't abort (un)installation on error - it just sets the error flag and skips the file as if nothing happened
		try:
		ClearErrors
		Delete $0
		${if} ${errors}
			MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION "Error deleting file:$\r$\n$\r$\n$0$\r$\n$\r$\nClick Retry to try again, or$\r$\nCancel to stop the uninstall." /SD IDCANCEL IDRETRY try
			Abort "Error deleting file $0" ; when called from section, will SetErrorLevel 2 - Installation aborted by script
		${endif}
	FunctionEnd
!macroend

!macro DeleteRetryAbort FILE_NAME
	Push $0
	
	StrCpy $0 "${FILE_NAME}"
	!ifndef __UNINSTALL__
		Call DeleteRetryAbort
	!else
	    Call un.DeleteRetryAbort
	!endif
	
	Pop $0
!macroend

!macro CheckSingleInstance TYPE SCOPE MUTEX_NAME
	Push $0
	Push $1
	Push $2
	
	StrCpy $0 "${TYPE}"
	StrCpy $1 "${SCOPE}"
	StrCpy $2 "${MUTEX_NAME}"
	!ifndef __UNINSTALL__
		Call CheckSingleInstance
	!else
		Call un.CheckSingleInstance
	!endif
	
	Pop $2
	Pop $1
	Pop $0
!macroend

!insertmacro DeleteRetryAbortFunc ""
!insertmacro CheckSingleInstanceFunc ""
