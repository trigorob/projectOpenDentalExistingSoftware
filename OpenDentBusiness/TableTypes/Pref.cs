﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenDentBusiness {
	///<summary>Stores small bits of data for a wide variety of purposes.  Any data that's too small to warrant its own table will usually end up here.</summary>
	[Serializable]
	[CrudTable(TableName="preference")]
	public class Pref:TableBase {
		///<summary>Primary key.</summary>
		[CrudColumn(IsPriKey=true)]
		public long PrefNum;
		///<summary>The text 'key' in the key/value pairing.</summary>
		public string PrefName;
		///<summary>The stored value.</summary>
		public string ValueString;
		///<summary>Documentation on usage and values of each pref.  Mostly deprecated now in favor of using XML comments in the code.</summary>
		public string Comments;

		///<summary>Returns a copy of the pref.</summary>
		public Pref Copy() {
			return (Pref)this.MemberwiseClone();
		}
	}

	///<summary>Because this enum is stored in the database as strings rather than as numbers, we can do the order alphabetically.  This enum must exactly match the prefs in the database.  Deprecated preferences will start with "Deprecated" in the summary section.</summary>
	public enum PrefName {
		AccountingCashIncomeAccount,
		AccountingDepositAccounts,
		AccountingIncomeAccount,
		AccountingLockDate,
		///<summary>Enum:AccountingSoftware 0=None, 1=Open Dental, 2=QuickBooks</summary>
		AccountingSoftware,
		///<summary>Defaulted to off, determines whether completed payment plans are visible in the account module.</summary>
		AccountShowCompletedPaymentPlans,
		AccountShowPaymentNums,
		///<summary>Show questionnaire button in account module toolbar.  Set in FormShowFeatures.</summary>
		AccountShowQuestionnaire,
		///<summary>Show TrojanCollect button in account module toolbar.  Set in FormShowFeatures.</summary>
		AccountShowTrojanExpressCollect,
		ADAComplianceDateTime,
		ADAdescriptionsReset,
		///<summary>When set to true, the user will not be able to save a new adjustment without first attaching a procedure to it.</summary>
		AdjustmentsForceAttachToProc,
		///<summary>Enum:ADPCompanyCode Used to generate the export file from FormTimeCardManage. Set in FormTimeCardSetup.</summary>
		ADPCompanyCode,
		///<summary>Stored as DateTime, but cleared when aging finishes.  The DateTime will be used as a flag to signal other connections that aging
		///calculations have started and prevents another connection from running simultaneously.  In order to run aging, this will have to be cleared,
		///either by the connection that set the flag when aging finishes, or by the user overriding the lock and manually clearing this pref.</summary>
		AgingBeginDateTime,
		AgingCalculatedMonthlyInsteadOfDaily,
		///<summary>If true, aging will use the intermediate table famaging for calculating aging.</summary>
		AgingIsEnterprise,
		///<summary>FK to allergydef.AllergyDefNum</summary>
		AllergiesIndicateNone,
		AllowedFeeSchedsAutomate,
		AllowSettingProcsComplete,
		AppointmentBubblesDisabled,
		AppointmentBubblesNoteLength,
		AppointmentClinicTimeReset,
		///<summary>Enum:SearchBehaviorCriteria 0=ProviderTime, 1=ProviderTimeOperatory</summary>
		AppointmentSearchBehavior,
		AppointmentTimeArrivedTrigger,
		AppointmentTimeDismissedTrigger,
		///<summary>The number of minutes that the appointment schedule is broken up into.  E.g. "10" represents 10 minute increments.</summary>
		AppointmentTimeIncrement,
		///<summary>Set to true if appointment times are locked by default.</summary>
		AppointmentTimeIsLocked,
		///<summary>Used to set the color of the time indicator line in the appt module.  Stored as an int.</summary>
		AppointmentTimeLineColor,
		AppointmentTimeSeatedTrigger,
		///<summary>Controls whether or not creating new appointments prompt to select an appointment type.</summary>
		AppointmentTypeShowPrompt,
		///<summary>Controls whether or not a warning will be displayed when selecting an appointment type would detach procedures from an appointment..</summary>
		AppointmentTypeShowWarning,
		ApptBubbleDelay,
		ApptExclamationShowForUnsentIns,
		///<summary>Boolean defaults to 0.  If true, adds the adjustment total to the net production in appointment module.</summary>
		ApptModuleAdjustmentsInProd,
		///<summary>Boolean defaults to 0, when true appt module will default to week view</summary>
		ApptModuleDefaultToWeek,
		///<summary>Boolean defaults to 1 if there is relevant ortho chart info, when true appt menu will have an ortho chart item.</summary>
		ApptModuleShowOrthoChartItem,
		///<summary>Keeps the waiting room indicator times current.  Initially 1.</summary>
		ApptModuleRefreshesEveryMinute,
		///<summary>Integer</summary>
		ApptPrintColumnsPerPage,
		///<summary>Float</summary>
		ApptPrintFontSize,
		///<summary>Stored as DateTime.  Currently the date portion is not used but might be used in future versions.</summary>
		ApptPrintTimeStart,
		///<summary>Stored as DateTime.  Currently the date portion is not used but might be used in future versions.</summary>
		ApptPrintTimeStop,
		///<summary>FKey to definition.DefNum.  If using automated confirmations, appointment set to this status when confirmation is sent.</summary>
		ApptEConfirmStatusSent,
		///<summary>FKey to definition.DefNum.  If using automated confirmations, appointment set to this status when confirmation is confirmed.</summary>
		ApptEConfirmStatusAccepted,
		///<summary>FKey to definition.DefNum.  If using automated confirmations, Anything that is not "Accepted" or "Sent".</summary>
		ApptEConfirmStatusDeclined,
		///<summary>FKey to definition.DefNum.  If using automated confirmations, when failed by HQ for some reason.</summary>
		ApptEConfirmStatusSendFailed,
		///<summary>True if automated appointment confirmations are enabled for the entire DB. See ApptReminderRules for setup details.
		///Permissions are still checked here at HQ so manually overriding this value will only make the program behave annoyingly, but won't break anything.</summary>
		ApptConfirmAutoEnabled,
		///<summary>Bool; Only if using clinics, when true causes automation to skip appointments not assigned to a clinic.</summary>
		ApptConfirmEnableForClinicZero,
		///<summary>Comma delimited list of FKey to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from EConfirmation RSVP updates.
		///Prevents overwriting manual Confirmation status.</summary>
		ApptConfirmExcludeEConfirm,
		///<summary>Comma delimited list of FKey to definition.DefNum. Every appointment with a confirmed status that is in this list will be excluded from sending an EConfirmation.
		///Prevents overwriting manual Confirmation status.</summary>
		ApptConfirmExcludeESend,
		///<summary>True if automated appointment reminders are enabled for the entire DB. See ApptReminderRules for setup details.
		///Permissions are still checked here at HQ so manually overriding this value will only make the program behave annoyingly, but won't break anything.</summary>
		ApptRemindAutoEnabled,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderDayInterval,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderDayMessage,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderEmailMessage,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderHourInterval,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderHourMessage,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderSendAll,
		///<summary>DEPRECATED.  See ApptReminderRule table instead.</summary>
		ApptReminderSendOrder,
		///<summary>Bool; False by default.  When true, the secondary provider used when scheduling an appointment will use the Operatory's secondary provider no matter what.</summary>
		ApptSecondaryProviderConsiderOpOnly,
		///<summary>Used by OD HQ.  Not added to db convert script.  Used to store the IP address of the asterisk phone server for the phone comms and voice mails.</summary>
		AsteriskServerIp,
		///<summary>Deprecated, but must remain here to avoid breaking updates.</summary>
		AtoZfolderNotRequired,
		///<summary>Enum - Enumerations.DataStorageType.  Normally 1 (AtoZ).  This used to be called AtoZfolderNotRequired, but that name was confusing.</summary>
		AtoZfolderUsed,
		///<summary>The number of audit trail entries that are displayed in the grid.</summary>
		AuditTrailEntriesDisplayed,
		///<summary>Used to determine the runtime of the threads that do automatic communication in the listener.  Stored as a DateTime.</summary>
		AutomaticCommunicationTimeStart,
		///<summary>Used to determine the runtime of the threads that do automatic communication in the listener.  Stored as a DateTime.</summary>
		AutomaticCommunicationTimeEnd,
		///<summary>Boolean.  Defaults to same value as ShowFeatureEhr.  Used to determine whether automatic summary of care webmails are sent.</summary>
		AutomaticSummaryOfCareWebmail,
		AutoResetTPEntryStatus,
		BackupExcludeImageFolder,
		BackupFromPath,
		BackupReminderLastDateRun,
		BackupRestoreAtoZToPath,
		BackupRestoreFromPath,
		BackupRestoreToPath,
		BackupToPath,
		BadDebtAdjustmentTypes,
		BalancesDontSubtractIns,
		BankAddress,
		BankRouting,
		BillingAgeOfAccount,
		BillingChargeAdjustmentType,
		BillingChargeAmount,
		BillingChargeLastRun,
		///<summary>Value is a string, either Billing or Finance.</summary>
		BillingChargeOrFinanceIsDefault,
		BillingDefaultsInvoiceNote,
		BillingDefaultsIntermingle,
		BillingDefaultsLastDays,
		BillingDefaultsNote,
		///<summary>Value is an integer, identifying the max number of statements that can be sent per batch.  Default of 0, which indicates no limit.
		///This preference is used for both printed statements and electronic ones.  It was decided to not rename the pref.</summary>
		BillingElectBatchMax,
		///<summary>Deprecated.  Use ebill.ClientAcctNumber instead.</summary>
		BillingElectClientAcctNumber,
		///<summary>Boolean, true by default.  Indicates if electronic billing should generate a PDF document.</summary>
		BillingElectCreatePDF,
		BillingElectCreditCardChoices,
		///<summary>Deprecated.  Use ebill.ElectPassword instead.</summary>
		BillingElectPassword,
		///<summary>No UI, can only be manually enabled by a programmer.  Only used for debugging electronic statements, because it will bloat the OpenDentImages folder.  Originally created to help with the "missing brackets bug" for EHG billing.</summary>
		BillingElectSaveHistory,
		///<summary>Output path for ClaimX EStatments.</summary>
		BillingElectStmtOutputPathClaimX,
		///<summary>Output path for EDS EStatments.</summary>
		BillingElectStmtOutputPathEds,
		///<summary>Output path for POS EStatments.</summary>
		BillingElectStmtOutputPathPos,
		///<summary>URL that EStatments are uploaded to for Dental X Change. Previously hardcoded in version 16.2.18 and below.</summary>
		BillingElectStmtUploadURL,
		///<summary>Deprecated.  Use ebill.ElectUserName instead.</summary>
		BillingElectUserName,
		BillingElectVendorId,
		BillingElectVendorPMSCode,
		BillingEmailBodyText,
		BillingEmailSubject,
		BillingExcludeBadAddresses,
		BillingExcludeIfUnsentProcs,
		BillingExcludeInactive,
		BillingExcludeInsPending,
		BillingExcludeLessThan,
		BillingExcludeNegative,
		BillingIgnoreInPerson,
		BillingIncludeChanged,
		///<summary>Used with repeat charges to apply repeat charges to patient accounts on billing cycle date.</summary>
		BillingUseBillingCycleDay,
		BillingSelectBillingTypes,
		///<summary>Boolean.  Defaults to true.  Determines if the billing window shows progress when sending statements.</summary>
		BillingShowSendProgress,
		///<summary>0=no,1=EHG,2=POS(xml file),3=ClaimX(xml file),4=EDS(xml file)</summary>
		BillingUseElectronic,
		BirthdayPostcardMsg,
		///<summary>FK to definition.DefNum.  The adjustment type that will be used on the adjustment that is automatically created when an appointment is broken.</summary>
		BrokenAppointmentAdjustmentType,
		///<summary>Boolean.  Default to true if D9986 is present.
		///When true, creates a broken appointment procedure when an appointment is broken.</summary>
		BrokenApptProcedure,
		///<summary>Deprecated.  Boolean.  0 by default.  When true, makes a commlog, otherwise makes an adjustment.</summary>
		BrokenApptCommLogNotAdjustment,
		///<summary>Boolean.  0 by default.  When true, makes a commlog when an appointment is broken.</summary>
		BrokenApptCommLog,
		///<summary>Boolean.  0 by default.  When true, makes an adjustment when an appointment is broken.</summary>
		BrokenApptAdjustment,
		///<summary>For Ontario Dental Association fee schedules.</summary>
		CanadaODAMemberNumber,
		///<summary>For Ontario Dental Association fee schedules.</summary>
		CanadaODAMemberPass,
		///<summary>Boolean.  0 by default.  If enabled, only CEMT can edit certain security settings.  Currently only used for global lock date.</summary>
		CentralManagerSecurityLock,
		///<summary>This is the hash of the password that is needed to open the Central Manager tool.</summary>
		CentralManagerPassHash,
		///<summary>Blank by default.  Contains a key for the CEMT.  Each CEMT database contains a unique sync code.  Syncing from the CEMT will skip any databases without the correct sync code.</summary>
		CentralManagerSyncCode,
		///<summary>Deprecated.</summary>
		ChartQuickAddHideAmalgam,
		///<summary>Deprecated. If set to true (1), then after adding a proc, a row will be added to datatable instead of rebuilding entire datatable by making queries to the database.
		///This preference was never fully implemented and should not be used.  We may revisit some day.</summary>
		ChartAddProcNoRefreshGrid,
		///<summary>Preference to warn users when they have a nonpatient selected.</summary>
		ChartNonPatientWarn,
		ClaimAttachExportPath,
		ClaimFormTreatDentSaysSigOnFile,
		///<summary>When true, the default ordering provider on medical eclaim procedures will be set to the procedure treating provider.</summary>
		ClaimMedProvTreatmentAsOrdering,
		ClaimMedTypeIsInstWhenInsPlanIsMedical,
		///<summary>For the Procedures Not Billed to Insurance report.  If true, when creating new claims from the report window, will group procedures
		///by clinic and site.  If false, will block user from creating claims if the selected procedures for a specific patient have different
		///clinis or different sites.  Default value is true to encourage automation.</summary>
		ClaimProcsNotBilledToInsAutoGroup,
		///<summary>Blank by default.  Computer name to receive reports from automatically.</summary>
		ClaimReportComputerName,
		///<summary>Report receive interval.  30 by default.</summary>
		ClaimReportReceiveInterval,
		///<summary>Stores last time the reports were ran.</summary>
		ClaimReportReceiveLastDateTime,
		///<summary>Boolean.  0 by default.  If enabled, the Send Claims window will automatically validate e-claims upon loading the window.
		///Validating all claims on load was old behavior that was significantly slowing down the loading of the send claims window.
		///Several offices complained that we took away the validation until they attempt sending the claim.</summary>
		ClaimsSendWindowValidatesOnLoad,
		///<summary>Boolean.  0 by default.  If enabled, snapshots of claimprocs are created when claims are created.</summary>
		ClaimSnapshotEnabled,
		///<summary>DateTime where the time is the only useful part. 
		///Stores the time of day that the eConnector should create a claimsnapshot.</summary>
		ClaimSnapshotRunTime,
		///<summary>Enumeration of type "ClaimSnapshotTrigger".  ClaimCreate by default.  This preference determines how ClaimSnapshots get created. Stored as the enumeration.ToString().</summary>
		ClaimSnapshotTriggerType,
		ClaimsValidateACN,
		ClearinghouseDefaultDent,
		///<summary>FK to clearinghouse.ClearingHouseNum.  Allows a different clearinghouse to be used for checking eligibility.
		///Defaults to the current dental (or medical) clearinghouse which preserves old behavior.</summary>
		ClearinghouseDefaultEligibility,
		ClearinghouseDefaultMed,
		///<summary>Boolean.  0 by default.  If enabled, lists clinics in alphabetical order.</summary>
		ClinicListIsAlphabetical,
		///<summary>String, "Workstation"(default), "User", "None". See FormMisc. Determines how recently viewed clinics should be tracked.</summary>
		ClinicTrackLast,
		ColorTheme,
		ConfirmEmailMessage,
		ConfirmEmailSubject,
		ConfirmPostcardMessage,
		///<summary>FK to definition.DefNum.  Initially 0.</summary>
		ConfirmStatusEmailed,
		///<summary>FK to definition.DefNum.</summary>
		ConfirmStatusTextMessaged,
		///<summary>The message that goes out to patients when doing a batch confirmation.</summary>
		ConfirmTextMessage,
		///<summary>Selected connection group within the CEMT.</summary>
		ConnGroupCEMT,
		CoPay_FeeSchedule_BlankLikeZero,
		///<summary>Boolean.  Typically set to true when an update is in progress and will be set to false when finished.  Otherwise true means that the database is in a corrupt state.</summary>
		CorruptedDatabase,
		///<summary>This is the default encounter code used for automatically generating encounters when specific actions are performed in Open Dental.  The code is displayed/set in FormEhrSettings.  We will set it and give the user a list of 9 suggested codes to use such that the encounters generated will cause the pateint to be considered part of the initial patient population in the 9 clinical quality measures tracked by OD.  CQMDefaultEncounterCodeSystem will identify the code system this code is from and the code value will be a FK to that code system.</summary>
		CQMDefaultEncounterCodeValue,
		CQMDefaultEncounterCodeSystem,
		CropDelta,
		///<summary>Used by OD HQ.  Not added to db convert script.  Allowable timeout for Negotiator to establish a connection with Listener. Different than SocketTimeoutMS and TransmissionTimeoutMS.  Specifies the allowable timeout for Patient Portal Negotiator to establish a connection with Listener.  Negotiator will only wait this long to get an acknowledgement that the Listener is available for a transmission before timing out.  Initially 10000</summary>
		CustListenerConnectionRequestTimeoutMS,
		///<summary>Used by OD HQ.  Not added to db convert script.  Will be passed to OpenDentalEConnector when service initialized.  Specifies the time (in minutes) between each time that the listener service will upload it's current heartbeat to HQ.  Initially 360</summary>
		CustListenerHeartbeatFrequencyMinutes,
		///<summary>Used by OpenDentalEConnector.  String specifies which port the OpenDentalWebService should look for on the customer's server in order to create a socket connection.  Initially 25255</summary>
		CustListenerPort,
		///<summary>Used by OD HQ.  Not added to db convert script.  Will be passed to OpenDentalEConnector when service initialized.  Specifies the read/write socket timeout.  Initially 3000</summary>
		CustListenerSocketTimeoutMS,
		///<summary>Used by OD HQ.  Not added to db convert script.  Specifies the entire wait time alloted for a transmission initiated by the patient portal.  Negotiator will only wait this long for a valid response back from Listener before timing out.  Initially 30000</summary>		
		CustListenerTransmissionTimeoutMS,
		CustomizedForPracticeWeb,
		DatabaseConvertedForMySql41,
		DataBaseVersion,
		DateDepositsStarted,
		DateLastAging,
		DefaultCCProcs,
		DefaultClaimForm,
		DefaultProcedurePlaceService,
		///<summary>Boolean.  Set to 1 to indicate that this database holds customers instead of patients.  Used by OD HQ.  Used for showing extra phone numbers, showing some extra buttons for tools that only we use, behavior of checkboxes in repeating charge window, etc.  But phone panel visibility is based on DockPhonePanelShow.</summary>
		DistributorKey,
		DockPhonePanelShow,
		///<summary>The AtoZ folder path.</summary>
		DocPath,
		///<summary>The ICD Diagnosis Code version primarily used by the practice.  Value of '9' for ICD-9, and '10' for ICD-10.</summary>
		DxIcdVersion,
		EasyBasicModules,
		/// <summary>Depricated.</summary>
		EasyHideAdvancedIns,
		EasyHideCapitation,
		EasyHideClinical,
		EasyHideDentalSchools,
		EasyHideHospitals,
		EasyHideInsurance,
		EasyHideMedicaid,
		EasyHidePrinters,
		EasyHidePublicHealth,
		EasyHideRepeatCharges,
		EasyNoClinics,
		EclaimsSeparateTreatProv,
		///<summary>Boolean, false by default.  Will be set to true when the update server successfully upgrades the CustListener service to the 
		///eConnector service.  This only needs to happen once.  This will automatically happen after upgrading past v15.4.
		///If automatically upgrading the CustListener service to the eConnector service fails, they can click Install in eService Setup.
		///NEVER programmatically set this preference back to false.</summary>
		EConnectorEnabled,
		EHREmailFromAddress,
		EHREmailPassword,
		EHREmailPOPserver,
		EHREmailPort,
		EhrRxAlertHighSeverity,
		///<summary>This pref is hidden, so no practical way for user to turn this on.  Only used for ehr testing.</summary>
		EHREmailToAddress,
		///<summary>Date when user upgraded to 13.1.14 and started using NewCrop Guids on Rxs.</summary>
		ElectronicRxDateStartedUsing131,
		/// <summary>FK to EmailAddress.EmailAddressNum.  It is not required that a default be set.</summary>
		EmailDefaultAddressNum,
		///<summary>The name of the only computer allowed to get new email messages from an email inbox (including Direct messages).</summary>
		EmailInboxComputerName,
		///<summary>Time interval in minutes describing how often to automatically check the email inbox for new messages. Default is 5 minutes.</summary>
		EmailInboxCheckInterval,
		///<summary>FK to EmailAddress.EmailAddressNum.  Used for webmail notifications (Patient Portal).</summary>
		EmailNotifyAddressNum,
		/// <summary>Deprecated. Use emailaddress.EmailPassword instead.</summary>
		EmailPassword,
		/// <summary>Deprecated. Use emailaddress.ServerPort instead.</summary>
		EmailPort,
		/// <summary>Deprecated. Use emailaddress.SenderAddress instead.</summary>
		EmailSenderAddress,
		/// <summary>Deprecated. Use emailaddress.SMTPserver instead.</summary>
		EmailSMTPserver,
		/// <summary>Deprecated. Use emailaddress.EmailUsername instead.</summary>
		EmailUsername,
		/// <summary>Deprecated. Use emailaddress.UseSSL instead.</summary>
		EmailUseSSL,
		/// <summary>Boolean. 0 means false and means it is not an EHR Emergency, and emergency access to the family module is not granted.</summary>
		EhrEmergencyNow,
		///<summary>There is no UI for this.  It's only used by OD HQ.</summary>
		EhrProvKeyGeneratorPath,
		EnableAnesthMod,
		///<summary>Warns the user if the Medicaid ID is not the proper number of digits for that state.</summary>
		EnforceMedicaidIDLength,
		ExportPath,
		///<summary>Allows guarantor access to all family health information in the patient portal.  Default is 1.</summary>
		FamPhiAccess,
		FinanceChargeAdjustmentType,
		FinanceChargeAPR,
		FinanceChargeAtLeast,
		FinanceChargeLastRun,
		FinanceChargeOnlyIfOver,
		FuchsListSelectionColor,
		FuchsOptionsOn,
		GenericEClaimsForm,
		///<summary>Has no UI.  Used to validate help support.  See the OpenDentalHelp project for more information on HelpKey.</summary>
		HelpKey,
		HL7FolderOut,
		HL7FolderIn,
		///<summary>Used by HQ. Projected onto wall displayed on top of FormMapHQ</summary>
		HQTriageCoordinator,
		///<summary>procedurelog.DiagnosticCode will be set to this for new procedures and complete procedures if this field was blank when set complete.
		///This can be an ICD-9 or an ICD-10.  In future versions, could be another an ICD-11, ICD-12, etc.</summary>
		ICD9DefaultForNewProcs,
		///<summary>3-state prefernce used in the image module, state definitions are:
		///0 = Expand the document tree each time the Images module is visited.
		///1 = Document tree collapses when patient changes.
		///2 = Document tree folders persistent expand/collapse per user.</summary>
		ImagesModuleTreeIsCollapsed,
		ImageWindowingMax,
		ImageWindowingMin,
		///<summary>Boolean.  False by default.  When enabled a fix is enabled within ODTextBox (RichTextBox) for foreign users that use 
		///a different language input methodology that requires the composition of symbols in order to display their language correctly.
		///E.g. the Korean symbol '역' (dur) will not display correctly inside ODTextBoxes without this set to true.</summary>
		ImeCompositionCompatibility,
		Ins834ImportPath,
		Ins834IsPatientCreate,
		///<summary>0=Default practice provider, -1=Treating Provider. Otherwise, FK to provider.ProvNum.</summary>
		InsBillingProv,
		InsDefaultCobRule,
		InsDefaultPPOpercent,
		InsDefaultShowUCRonClaims,
		InsDefaultAssignBen,
		///<summary>0=unknown, user did not make a selection.  1=Yes, 2=No.</summary>
		InsPlanConverstion_7_5_17_AutoMergeYN,
		///<summary>Boolean.  False by default.  When enabled, procedure fees will always use the UCR fee.</summary>
		InsPpoAlwaysUseUcrFee,
		///<summary>0 by default.  If false, secondary PPO writeoffs will always be zero (normal).  At least one customer wants to see secondary writeoffs.</summary>
		InsPPOsecWriteoffs,
		InsTpChecksFrequency,
		InsurancePlansShared,
		///<summary>7 by default.  Number of days before displaying insurances that need verified.</summary>
		InsVerifyAppointmentScheduledDays,
		///<summary>90 by default. Number of days before requiring insurance plans to be verified.</summary>
		InsVerifyBenefitEligibilityDays,
		///<summary>Boolean, false by default.  When true, defaults a filter to the current user instead of All when opening the InsVerifyList.</summary>
		InsVerifyDefaultToCurrentUser,
		///<summary>Boolean, false by default.  When true, excludes patient clones from the Insurance Verification List.</summary>
		InsVerifyExcludePatientClones,
		///<summary>Boolean, false by default.  When true, excludes patient plans associated to insurance plans that are marked "Do Not Verify" from the Insurance Verification List.</summary>
		InsVerifyExcludePatVerify,
		///<summary>30 by default.  Number of days before requiring patient plans to be verified.</summary>
		InsVerifyPatientEnrollmentDays,
		///<summary>Writeoff description displayed in the Account Module and on statements.  If blank, the default is "Writeoff".
		///We are using "Writeoff" since "PPO Discount" was only used for a brief time in 15.3 while it was Beta and no customer requested it</summary>
		InsWriteoffDescript,
		IntermingleFamilyDefault,
		JobManagerDefaultEmail,
		JobManagerDefaultBillingMsg,
		LabelPatientDefaultSheetDefNum,
		///<summary>Used to determine how many windows are displayed throughout the program, translation, charting, and other features. Version 15.4.1</summary>
		LanguageAndRegion,
		///<summary>Initially set to Declined to Specify.  Indicates which language from the LanguagesUsedByPatients preference is the language that indicates the patient declined to specify.  Text must exactly match a language in the list of available languages.  Can be blank if the user deletes the language from the list of available languages.</summary>
		LanguagesIndicateNone,
		///<summary>Comma-delimited list of two-letter language names and custom language names.  The custom language names are the full string name and are not necessarily supported by Microsoft.</summary>
		LanguagesUsedByPatients,
		LetterMergePath,
    ///<summary>Boolean. Only used to override server time in the following places: Time Cards.</summary>
    LocalTimeOverridesServerTime,
		MainWindowTitle,
		///<summary>0=Meaningful Use Stage 1, 1=Meaningful Use Stage 2.  Global, affects all providers.  Changes the MU grid that is seen for individual patients and for summary reports.</summary>
		MeaningfulUseTwo,
		///<summary>Number of days after medication order start date until stop date.  Used when automatically inserting a medication order when creating
		///a new Rx.  Default value is 7 days.  If set to 0 days, the automatic stop date will not be entered.</summary>
		MedDefaultStopDays,
		///<summary>New procs will use the fee amount tied to the medical code instead of the ADA code.</summary>
		MedicalFeeUsedForNewProcs,
		///<summary>FK to medication.MedicationNum</summary>
		MedicationsIndicateNone,
		///<summary>If MedLabReconcileDone=="0", a one time reconciliation of the MedLab HL7 messages is needed. The reconcile will reprocess the original
		///HL7 messages for any MedLabs with PatNum=0 in order to create the embedded PDF files from the base64 text in the ZEF segments. The old method
		///of waiting to extract these files until the message is manually attached to a patient was very slow using the middle tier. The new method is to
		///create the PDF files and save them in the image folder in a subdirectory called "MedLabEmbeddedFiles" if a pat is not located from the details
		///in the PID segment of the message. Attaching the MedLabs to a patient is now just a matter of moving the files to the patient's image folder.
		///All files will now be extracted and stored, either in a pat's folder or in the "MedLabEmbeddedFiles" folder, by the HL7 service.</summary>
		MedLabReconcileDone,
		MobileSyncDateTimeLastRun,
		///<summary>Used one time after the conversion to 7.9 for initial synch of the provider table.</summary>
		MobileSynchNewTables79Done,
		///<summary>Used one time after the conversion to 11.2 for re-synch of the patient records because a)2 columns BalTotal and InsEst have been added to the patientm table. b) the table documentm has been added</summary>
		MobileSynchNewTables112Done,
		///<summary>Used one time after the conversion to 12.1 for the recallm table being added and for upload of the practice Title.</summary>
		MobileSynchNewTables121Done,
		MobileSyncIntervalMinutes,
		MobileSyncServerURL,
		MobileSyncWorkstationName,
		MobileExcludeApptsBeforeDate,
		MobileUserName,
		//MobileSyncLastFileNumber,
		//MobileSyncPath,
		///<summary>The major and minor version of the current MySQL connection.  Gets updated on startup when a new version is detected.</summary>
		MySqlVersion,
		///<summary>True by default.  Will use the claimsnapshot table for calculating production in the Net Production Detail report if the date range is today's date only.</summary>
		NetProdDetailUseSnapshotToday,
		///<summary>There is no UI for user to change this.  Format, if OD customer, is PatNum-(RandomString)(CheckSum).  Example: 1234-W6c43.  Format for resellers is up to them.</summary>
		NewCropAccountId,
		///<summary>The date this customer last checked with HQ to determine which provider have access to eRx.</summary>
		NewCropDateLastAccessCheck,
		///<summary>True for customers who were using NewCrop before version 15.4.  True if NewCropAccountId was not blank when upgraded.</summary>
		NewCropIsLegacy,
		///<summary>Controls which NewCrop database to use.  If false, then the customer uses the First Data Bank (FDB) database, otherwise the 
		///customer uses the LexiData database.  Connecting to LexiData saves NewCrop some money on the new accounts.  Additionally, the RxNorms which
		///come back from the prescription refresh in the Chart are more complete for the LexiData database than for the FDB database.</summary>
		NewCropIsLexiData,
		///<summary>There is no UI for user to change this. For distributors, this is part of the credentials.  OD credentials are not stored here, but are hard-coded.</summary>
		NewCropName,
		///<summary>There is no UI for user to change this.  For distributors, this is part of the credentials.
		///OD credentials are not stored here, but are hard-coded.</summary>
		NewCropPartnerName,
		///<summary>There is no UI for user to change this.  For distributors, this is part of the credentials.
		///OD credentials are not stored here, but are hard-coded.</summary>
		NewCropPassword,
		///<summary>URL of the time server to use for EHR time synchronization.  Only used for EHR.  Example nist-time-server.eoni.com</summary>
		NistTimeServerUrl,
		OpenDentalVendor,
		OracleInsertId,
		PasswordsMustBeStrong,
		PatientAllSuperFamilySync,
		PatientFormsShowConsent,
		///<summary>Free-form 'Body' text of the notification sent by this practice when a new secure EmailMessage is sent to patient.</summary>
		PatientPortalNotifyBody,
		///<summary>Free-form 'Subject' text of the notification sent by this practice when a new secure EmailMessage is sent to patient.</summary>
		PatientPortalNotifySubject,
		PatientPortalURL,
		PatientSelectUseFNameForPreferred,
		///<summary>Boolean. This is the default for new computers, otherwise it uses the computerpref PatSelectSearchMode.</summary>
		PatientSelectUsesSearchButton,
		///<summary>PaySplitManager enum. 1 by default. 0=DoNotUse, 1=Prompt, 2=Force</summary>
		PaymentsPromptForAutoSplit,
		///<summary>0 by default.1=Prompt users to select payment type when creating new Payments.</summary>
		PaymentsPromptForPayType,
		///<summary>Boolean. True by default.  If false, Payment clinic will be determined by FormOpenDental.</summary>
		PaymentsUsePatientClinic,
		///<summary>Int.  Represents PayPeriodInterval enum (Weekly, Bi-Weekly, Monthly). </summary>
		PayPeriodIntervalSetting,
		///<summary>Int.  If set, represents the number of days after the pay period the pay day is.</summary>
		PayPeriodPayAfterNumberOfDays,
		///<summary>Boolean.  True by default.  If true, pay days will fall before weekends.  If false, pay days will fall after weekends.</summary>
		PayPeriodPayDateBeforeWeekend,
		///<summary>Boolean.  True by default.  Pay Day cannot fall on weekend if true.</summary>
		PayPeriodPayDateExcludesWeekends,
		///<summary>Int. If set to 0, it's disabled, but any other number represents a day of the week. 1:Sunday, 2:Monday etc...</summary>
		PayPeriodPayDay,
		PayPlansBillInAdvanceDays,
		///<summary>The Payment Plan version that the customer is using.  Value values are 1 and 2.  1 by default.</summary>
		PayPlansUseSheets,
		PayPlansVersion,
		PerioColorCAL,
		PerioColorFurcations,
		PerioColorFurcationsRed,
		PerioColorGM,
		PerioColorMGJ,
		PerioColorProbing,
		PerioColorProbingRed,
		PerioRedCAL,
		PerioRedFurc,
		PerioRedGing,
		PerioRedMGJ,
		PerioRedMob,
		PerioRedProb,
		///<summary>Enabled by default.  When a new perio exam is created, will always mark all missing teeth as skipped.</summary>
		PerioSkipMissingTeeth,
		///<summary>Enabled by default.  When a tooth with an implant procedure completed will not be skipped on perio exams</summary>
		PerioTreatImplantsAsNotMissing,
		PlannedApptTreatedAsRegularAppt,
		PracticeAddress,
		PracticeAddress2,
		PracticeBankNumber,
		PracticeBillingAddress,
		PracticeBillingAddress2,
		PracticeBillingCity,
		PracticeBillingST,
		PracticeBillingZip,
		PracticeCity,
		PracticeDefaultBillType,
		PracticeDefaultProv,
		///<summary>In USA and Canada, enforced to be exactly 10 digits or blank.</summary>
		PracticeFax,
		///<summary>This preference is used to hide/change certain OD features, like hiding the tooth chart and changing 'dentist' to 'provider'.</summary>
		PracticeIsMedicalOnly,
		PracticePayToAddress,
		PracticePayToAddress2,
		PracticePayToCity,
		PracticePayToST,
		PracticePayToZip,
		///<summary>In USA and Canada, enforced to be exactly 10 digits or blank.</summary>
		PracticePhone,
		PracticeST,
		PracticeTitle,
		PracticeZip,
		///<summary>This is the default pregnancy code used for diagnosing pregnancy from FormVitalSignEdit2014 and is displayed/set in FormEhrSettings.  When the check box for BMI and BP not taken due to pregnancy Dx is selected, this code value will be inserted into the diseasedef table in the column identified by the PregnancyDefaultCodeSystem (i.e. diseasedef.SnomedCode, diseasedef.ICD9Code).  It will then be a FK in the diseasedef table to the associated code system table.</summary>
		PregnancyDefaultCodeValue,
		PregnancyDefaultCodeSystem,
		///<summary>FK to definition.DefNum for PaySplitUnearnedType defcat (29)</summary>
		PrepaymentUnearnedType,
		///<summary>In Patient Edit and Add Family windows, the Primary Provider defaults to 'Select Provider' instead of the practice provider.</summary>
		PriProvDefaultToSelectProv,
		///<summary>FK to diseasedef.DiseaseDefNum</summary>
		ProblemsIndicateNone,
		ProblemListIsAlpabetical,
		///<summary>Determines default sort order of Proc Codes list when accessed from Lists -> Procedure Codes.  Enum:ProcCodeListSort, 0 by default.</summary>
		ProcCodeListSortOrder,
		///<summary>In FormProcCodes, this is the default for the ShowHidden checkbox.</summary>
		ProcCodeListShowHidden,
		///<summary>Users must use suggested auto codes for a procedure.</summary>
		ProcEditRequireAutoCodes,
		ProcLockingIsAllowed,
		///<summary>True by default.  Allows for substituting AutoNote text for [[text]] segments in a procedure's default note.</summary>
		ProcPromptForAutoNote,
		///<summary>Frequency at which signals are processed. Also used by HQ to determine triage label refresh frequency.</summary>		
		ProcessSigsIntervalInSecs,
		ProcGroupNoteDoesAggregate,
		///<summary>DateTime.  Next date that the advertising programming properties will automatically check.</summary>		
		ProgramAdditionalFeatures,
		///<summary>Deprecated. Use updatehistory table instead.  Stored the DateTime of when the ProgramVersion preference last changed.</summary>		
		ProgramVersionLastUpdated,
		ProgramVersion,
		ProviderIncomeTransferShows,
		///<summary>Bool.  Defaults to true.  When true, allow the Provider Payroll report to select Today's date in the date range.</summary>		
		ProviderPayrollAllowToday,
		///<summary>Was never used.  Was supposed to indicate FK to sheet.Sheet_DEF_Num, so not even named correctly. Must be an exam sheet. Only makes sense if PublicHealthScreeningUsePat is true.</summary>
		PublicHealthScreeningSheet,
		///<summary>Was never used.  Always 0.  Boolean. Work for attaching to patients stopped 11/30/2012, there is currently no access to change the value of this preference.    When in this mode, screenings will be attached to actual PatNums rather than just freeform text names.</summary>
		PublicHealthScreeningUsePat,
		///<summary>Boolean, off by default.  Some users have clinics enabled but do not want QuickBooks to itemize their accounts.
		///Class Refs are a way for QuickBooks to itemize if set up correctly.</summary>
		QuickBooksClassRefsEnabled,
		QuickBooksCompanyFile,
		QuickBooksDepositAccounts,
		QuickBooksIncomeAccount,
		///<summary>Date when user upgraded to or past 15.4.1 and started using ADA procedures to count CPOE radiology orders for EHR.</summary>
		RadiologyDateStartedUsing154,
		///<summary>Boolean.  True if random primary keys have been turned on.
		///Causes all CRUD classes to look for an unused random PK before inserting instead of leaving it up to auto incrementing.</summary>
		RandomPrimaryKeys,
		RecallAdjustDown,
		RecallAdjustRight,
		///<summary>Defaults to 12 for new customers.  The number in this field is considered adult.  Only used when automatically adding procedures to a new recall appointment.</summary>
		RecallAgeAdult,
		RecallCardsShowReturnAdd,
		///<summary>-1 indicates min for all dates</summary>
		RecallDaysFuture,
		///<summary>-1 indicates min for all dates</summary>
		RecallDaysPast,
		RecallEmailFamMsg,
		RecallEmailFamMsg2,
		RecallEmailFamMsg3,
		RecallEmailMessage,
		RecallEmailMessage2,
		RecallEmailMessage3,
		RecallEmailSubject,
		RecallEmailSubject2,
		RecallEmailSubject3,
		RecallExcludeIfAnyFutureAppt,
		RecallGroupByFamily,
		///<summary> long. -1=infinite, 0=zero; if stored as -1, displays as "".</summary>
		RecallMaxNumberReminders,
		RecallPostcardFamMsg,
		RecallPostcardFamMsg2,
		RecallPostcardFamMsg3,
		RecallPostcardMessage,
		RecallPostcardMessage2,
		RecallPostcardMessage3,
		RecallPostcardsPerSheet,
		RecallShowIfDaysFirstReminder,
		RecallShowIfDaysSecondReminder,
		RecallStatusEmailed,
		RecallStatusMailed,
		///<summary>Used if younger than 12 on the recall date.</summary>
		RecallTypeSpecialChildProphy,
		RecallTypeSpecialPerio,
		RecallTypeSpecialProphy,
		///<summary>Comma-delimited list. FK to recalltype.RecallTypeNum.</summary>
		RecallTypesShowingInList,
		///<summary>If false, then it will only use email in the recall list if email is the preferred recall method.</summary>
		RecallUseEmailIfHasEmailAddress,
		///<summary>Bool, 0 by default.  When true, recurring charges will use the primary provider of the patient when creating paysplits.
		///When false, the provider that the family is most in debt to will be used.</summary>
		RecurringChargesUsePriProv,
		RegistrationKey,
		RegistrationKeyIsDisabled,
		RegistrationNumberClaim,
		RenaissanceLastBatchNumber,
		///<summary>If replication has failed, this indicates the server_id.  No computer will be able to connect to this single server until this flag is cleared.</summary>
		ReplicationFailureAtServer_id,
		///<summary>The PK of the replication server that is flagged as the "report server".  If using replication, "create table" or "drop table" commands can only be executed within the user query window when connected to this server.</summary>
		ReplicationUserQueryServer,
		ReportFolderName,
    ///<summary>Boolean, on by default.</summary>
    ReportPandIhasClinicBreakdown,
    ///<summary>Boolean, off by default.</summary>
		ReportPandIhasClinicInfo,
		ReportPandIschedProdSubtractsWO,
		///<summary>Bool.  False by defualt, used to filter incomplete procedures by having no note in the Incomplete Procedures Report.</summary>
		ReportsIncompleteProcsNoNotes,
		///<summary>Bool.  False by defualt, used to filter incomplete procedures by having a note that is unsigned in the Incomplete Procedures Report.</summary>
		ReportsIncompleteProcsUnsigned,
		ReportsPPOwriteoffDefaultToProcDate,
		///<summary>Bool.  False by defualt, used to wrap columns when printing a custom report.</summary>
		ReportsWrapColumns,
		///<summary>Bool.  False by defualt, used to determine whether the reports progress bar will show a history or not.</summary>
		ReportsShowHistory,
		ReportsShowPatNum,
		RequiredFieldColor,
		RxSendNewToQueue,
		SalesTaxPercentage,
		ScannerCompression,
		ScannerResolution,
		ScannerSuppressDialog,
		///<summary>Set to 1 by default. Selects all providers/employees when loading schedules.</summary>
		ScheduleProvEmpSelectAll,
		ScheduleProvUnassigned,
		///<summary>Boolean. Off by default so that users will have to opt into utilizing the screening with sheets feature.
		///Screening with sheets is extremely customized for Dental3 (D3)</summary>
		ScreeningsUseSheets,
		///<summary>UserGroupNum for Instructors.  Set only for dental schools in dental school setup.</summary>
		SecurityGroupForInstructors,
		///<summary>UserGroupNum for Students.  Set only for dental schools in dental school setup.</summary>
		SecurityGroupForStudents,
		SecurityLockDate,
		///<summary>Set to 0 to always grant permission. 1 means only today.</summary>
		SecurityLockDays,
		SecurityLockIncludesAdmin,
		///<summary>Set to 0 to disable auto logoff.</summary>
		SecurityLogOffAfterMinutes,
		SecurityLogOffWithWindows,
		ShowAccountFamilyCommEntries,
		ShowFeatureEhr,
		///<summary>Set to 1 by default.  Shows a button in Edit Patient Information that lets users launch Google Maps.</summary>
		ShowFeatureGoogleMaps,
		ShowFeatureMedicalInsurance,
		///<summary>Set to 1 to enable the Synch Clone button in the Family module which allows users to create and synch clones of patients.</summary>
		ShowFeaturePatientClone,
		ShowFeatureSuperfamilies,
		///<summary>0=None, 1=PatNum, 2=ChartNumber, 3=Birthdate</summary>
		ShowIDinTitleBar,
		ShowProgressNotesInsteadofCommLog,
		///<summary>Deprecated.  Was used to hide the provider payroll report before users had the ability to remove it from the production and income listbox.</summary>
		ShowProviderPayrollReport,
		ShowUrgFinNoteInProgressNotes,
		///<summary>Used to stop signals after a period of inactivity.  A value of 0 disables this feature.  Default value of 0 to maintain backward compatibility</summary>
		SignalInactiveMinutes,
		///<summary>Only used on startup.  The date in which stale signalods were removed.</summary>
		SignalLastClearedDate,
		///<summary>Blank if not signed. Date signed. For practice level contract, if using clinics see Clinic.SmsContractDate. Record of signing also kept at HQ.</summary>
		SmsContractDate,
		///<summary>(Deprecated) Blank if not signed. Name signed. For practice level contract, if using clinics see Clinic.SmsContractName. Record of signing also kept at HQ.</summary>
		SmsContractName,
		///<summary>Always stored in US dollars. This is the desired limit for SMS outbound messages per month.</summary>
		SmsMonthlyLimit,
		/// <summary>Name of this Software.  Defaults to 'Open Dental Software'.</summary>
		SoftwareName,
		SolidBlockouts,
		SpellCheckIsEnabled,
		StatementAccountsUseChartNumber,
		StatementsCalcDueDate,
		StatementShowCreditCard,
		///<summary>Show payment notes.</summary>
		StatementShowNotes,
		StatementShowAdjNotes,
		StatementShowProcBreakdown,
		StatementShowReturnAddress,
		///<summary>Deprecated.  Not used anywhere.</summary>
		StatementSummaryShowInsInfo,
		StatementsUseSheets,
		///<summary>Used by OD HQ. Indicates whether WebCamOD applications should be sending their images to the server or not.</summary>
		StopWebCamSnapshot,
		StoreCCnumbers,
		StoreCCtokens,
		SubscriberAllowChangeAlways,
		SuperFamSortStrategy,
		SuperFamNewPatAddIns,
		TaskAncestorsAllSetInVersion55,
		TaskListAlwaysShowsAtBottom,
		///<summary>Deprecated.  Not used anywhere.  Previously used for the popup window to show how many new tasks for cur user after login.</summary>
		TasksCheckOnStartup,
		///<summary>If true use task.Status to determine if task is new. Otherwise use task.IsUnread.</summary>
		TasksNewTrackedByUser,
		TasksShowOpenTickets,
		///<summary>Boolean.  0 by default.  Sets appointment task lists to use special logic to sort by AptDateTime.</summary>
		TaskSortApptDateTime,
		///<summary>Boolean.  Defaults to false to hide repeating tasks feature if no repeating tasks are in use when updating to 16.3.</summary>
		TasksUseRepeating,
		///<summary>Keeps track of date of one-time cleanup of temp files.  Prevents continued annoying cleanups after the first month.</summary>
		TempFolderDateFirstCleaned,
		TerminalClosePassword,
		///<summary>If true, treat Yes-No-Unknown status of Unknown as if it were a No.</summary>
		TextMsgOkStatusTreatAsNo,
		TextingDefaultClinicNum,
		TimeCardADPExportIncludesName,
		///<summary>0=Sun,1=Mon...6=Sat</summary>
		TimeCardOvertimeFirstDayOfWeek,
		TimecardSecurityEnabled,
		///<summary>Boolean.  0 by default.  When enabled, FormTimeCard and FormTimeCardMange display H:mm:ss instead of HH:mm</summary>
		TimeCardShowSeconds,
		TimeCardsMakesAdjustmentsForOverBreaks,
		///<summary>bool</summary>
		TimeCardsUseDecimalInsteadOfColon,
		TimecardUsersDontEditOwnCard,
		TitleBarShowSite,
		///<summary>Deprecated.  Not used anywhere.</summary>
		ToothChartMoveMenuToRight,
		TreatmentPlanNote,
		TreatPlanDiscountAdjustmentType,
		///<summary>Set to 0 to clear out previous discounts.</summary>
		TreatPlanDiscountPercent,
		TreatPlanItemized,
		TreatPlanPriorityForDeclined,
		///<summary>When a TP is signed a PDF will be generated and saved. If disabled, TPs will be redrawn with current data (pre 15.4 behavior).</summary>
		TreatPlanSaveSignedToPdf,
		TreatPlanShowCompleted,
		TreatPlanShowGraphics,
		TreatPlanShowIns,
		///<summary>This preference merely defines what FormOpenDental.IsTreatPlanSortByTooth is on startup.
		///When true, procedures in the treatment plan module sort by priority, date, toothnum, surface, then PK. 
		///When false, does not sort by toothnum or surface. True by default to preserve old behavior.</summary>
		TreatPlanSortByTooth,
		TreatPlanUseSheets,
		TrojanExpressCollectBillingType,
		TrojanExpressCollectPassword,
		TrojanExpressCollectPath,
		TrojanExpressCollectPreviousFileNumber,
		UpdateCode,
		UpdateInProgressOnComputerName,
		///<summary>Described in the Update Setup window and in the manual.  Can contain multiple db names separated by commas.  Should not include current db name.</summary>
		UpdateMultipleDatabases,
		UpdateServerAddress,
		UpdateShowMsiButtons,
		///<summary>The next update date and time, set in FormUpdateSetup.  When this is set in the future, the main form's title bar will count down to the set time.</summary>
		UpdateDateTime,
		///<summary>Use GetStringNoCache() to get the value of this preference.</summary>
		UpdateStreamLinePassword,
		UpdateWebProxyAddress,
		UpdateWebProxyPassword,
		UpdateWebProxyUserName,
		UpdateWebsitePath,
		UpdateWindowShowsClassicView,
		UseBillingAddressOnClaims,
		///<summary>Enum:ToothNumberingNomenclature 0=Universal(American), 1=FDI, 2=Haderup, 3=Palmer</summary>
		UseInternationalToothNumbers,
		///<summary>Boolean.  0 by default.  When enabled, users must enter their user name manually at the log on window.</summary>
		UserNameManualEntry,
		///<summary>Boolean. 0 by default. When enabled, chart module procedures that are complete will use the provider's color as row's background color</summary>
		UseProviderColorsInChart,
		WaitingRoomAlertColor,
		///<summary>0 to disable.  When enabled, sets rows to alert color based on wait time.</summary>
		WaitingRoomAlertTime,
		///<summary>Boolean.  0 by default.  When enabled, the waiting room will filter itself by the selected appointment view.  0, normal filtering, will show all patients waiting for the entire practice (or entire clinic when using clinics).</summary>
		WaitingRoomFilterByView,
		///<summary>DEPRECATED.  Used by OD HQ.  Not added to db convert script.  No UI to change this value.
		///Determines how often in milliseconds that WebCamOD should capture and send a picture to the phone table.
		///If this value is manually changed, all Web Cams need to be restarted for the change to take effect.</summary>
		WebCamFrequencyMS,
		///<summary>Only used for sheet synch.  See Mobile... for URL for mobile synch.</summary>
		WebHostSynchServerURL,
		///<summary>Stored as an int value from the WebSchedAutomaticSend enum.</summary>
		WebSchedAutomaticSendSetting,
		WebSchedMessage,
		WebSchedMessage2,
		WebSchedMessage3,
		///<summary>Boolean.  0 by default.  True when the New Patient Appointment version of Web Sched is enabled.
		///Loosely keeps track of service status, calling our web service to verify active service is still required.</summary>
		WebSchedNewPatApptEnabled,
		///<summary>Comma delimited list of procedures that should be put onto the new patient appointment.</summary>
		WebSchedNewPatApptProcs,
		///<summary>The time pattern that will be used to determine the length of the new patient appointment.</summary>
		WebSchedNewPatApptTimePattern,
		///<summary>Integer.  Represents the number of days into the future we will go before searching for available time slots.
		///Empty will start looking for available time slots today.</summary>
		WebSchedNewPatApptSearchAfterDays,
		///<summary>Enum: WebSchedProviderRules 0=FirstAvailable, 1=PrimaryProvider, 2=SecondaryProvider, 3=LastSeenHygienist</summary>
		WebSchedProviderRule,
		///<summary>Boolean. 0 by default. True when Web Sched service is enabled.
		///Loosely keeps track of service status, calling our web service to verify active service is still required.
		///Used to make the UI of Open Dental different and less annoying (advertising wise) depeding on if the service is enabled or not.</summary>
		WebSchedService,
		WebSchedSubject,
		WebSchedSubject2,
		WebSchedSubject3,
		WebServiceHQServerURL,
		WebServiceServerName,
		///<summary>If enabled, allows users to right click on ODTextboxes or ODGrids to populate the context menu with any detected wiki links.</summary>
		WikiDetectLinks,
		///<summary>If enabled, allows users to create new wiki pages when following links from textboxes and grids. (Disable to prevent proliferation of misspelled wiki pages.)</summary>
		WikiCreatePageFromLink,
		WordProcessorPath,
		XRayExposureLevel
	}

	///<summary>Used by pref "AppointmentSearchBehavior". </summary>
	public enum SearchBehaviorCriteria {
		ProviderTime,
		ProviderTimeOperatory
	}

	///<summary>Used by pref "AccountingSoftware".  0=OpenDental, 1=QuickBooks</summary>
	public enum AccountingSoftware {
		OpenDental,
		QuickBooks
	}

	///<summary>Used by pref "WebSchedProviderRule". Determines how Web Sched will decide on what provider time slots to show patients.</summary>
	public enum WebSchedProviderRules {
		///<summary>0 - Dynamically picks the first available provider based on the time slot picked by the patient.</summary>
		FirstAvailable,
		///<summary>1 - Only shows time slots that are available via the patient's primary provider.</summary>
		PrimaryProvider,
		///<summary>2 - Only shows time slots that are available via the patient's secondary provider.</summary>
		SecondaryProvider,
		///<summary>3 - Only shows time slots that are available via the patient's last seen hygienist.</summary>
		LastSeenHygienist
	}
	



}
