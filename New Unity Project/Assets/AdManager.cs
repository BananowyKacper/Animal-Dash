using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour {

	public bool TestAds = false;

	private static BannerView bannerView;
	private InterstitialAd interstitialView;
	RewardBasedVideoAd rewardBasedVideoAd;


	public static bool firstTime = true;

	public static AdManager admanagerInstance = null;
	[SerializeField] private string appID = "";
	[SerializeField] private string bannerID = "ca-app-pub-3940256099942544/6300978111";
	[SerializeField] private string interstitialID = "ca-app-pub-3940256099942544/1033173712";
	private string rewardVideoID = "";

	void Awake()
	{		
		if (admanagerInstance == null) {
			admanagerInstance = this;
		} else if (admanagerInstance != this) {
			Destroy (gameObject);
		}

		if (firstTime) {
			firstTime = false;
			DontDestroyOnLoad (gameObject);		

			MobileAds.Initialize (appID);
			RequestInterstitial ();
			admanagerInstance.RequestBanner ();
		}
	}

	void Start()
	{
		// Called when an ad request has successfully loaded.
		bannerView.OnAdLoaded += HandleOnAdLoaded;
		// Called when an ad request failed to load.
		bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
		// Called when an ad is clicked.
		bannerView.OnAdOpening += HandleOnAdOpened;
		// Called when the user returned from the app after an ad click.
		bannerView.OnAdClosed += HandleOnAdClosed;
		// Called when the ad click caused the user to leave the application.
		bannerView.OnAdLeavingApplication += HandleOnAdLeavingApplication;

        /*
		//Reward
		// Get singleton reward based video ad reference.
		admanagerInstance.rewardBasedVideoAd = RewardBasedVideoAd.Instance;

		//Video Ad Events
		// Called when an ad request has successfully loaded.
		rewardBasedVideoAd.OnAdLoaded += HandleRewardBasedVideoLoaded;
		// Called when an ad request failed to load.
		rewardBasedVideoAd.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
		// Called when an ad is shown.
		rewardBasedVideoAd.OnAdOpening += HandleRewardBasedVideoOpened;
		// Called when the ad starts to play.
		rewardBasedVideoAd.OnAdStarted += HandleRewardBasedVideoStarted;
		// Called when the user should be rewarded for watching a video.
		rewardBasedVideoAd.OnAdRewarded += HandleRewardBasedVideoRewarded;
		// Called when the ad is closed.
		rewardBasedVideoAd.OnAdClosed += HandleRewardBasedVideoClosed;
		// Called when the ad click caused the user to leave the application.
		rewardBasedVideoAd.OnAdLeavingApplication += HandleRewardBasedVideoLeftApplication;

		//request reward video
		admanagerInstance.RequestRewardBasedVideo();
        */
	}

	#region AdmobBannerCallBackEvents
	public void HandleOnAdLoaded(object sender, EventArgs args)
	{
		OnClickShowBanner ();
		MonoBehaviour.print("HandleAdLoaded event received");
	}

	public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		//		RequestBanner ();
		MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
			+ args.Message);
	}

	public void HandleOnAdOpened(object sender, EventArgs args)
	{
		MonoBehaviour.print("HandleAdOpened event received");
	}

	public void HandleOnAdClosed(object sender, EventArgs args)
	{
		//		bannerView.Destroy ();
		MonoBehaviour.print("HandleAdClosed event received");
	}

	public void HandleOnAdLeavingApplication(object sender, EventArgs args)
	{
		MonoBehaviour.print("HandleAdLeavingApplication event received");
	}
	#endregion 	




	public void OnClickShowBanner()
	{
		bannerView.Show();
	}

	public void OnClickHideBanner()
	{
		bannerView.Hide ();
	}

	//Call this to call interstitial ad
	public void ShowInterstitial()
	{
		if (admanagerInstance.interstitialView.IsLoaded()) 
			admanagerInstance.interstitialView.Show();

		RequestInterstitial ();
	}

	public void ShowAdmobRewardVideo()
	{
		if (rewardBasedVideoAd.IsLoaded()) {
			rewardBasedVideoAd.Show();
			//When completed this function is called : HandleRewardBasedVideoRewarded
		}
	}


	#region AdmobRequests
	private void RequestBanner()
	{

		AdRequest request = null;
        if (!TestAds)
        {
            bannerView = new BannerView(bannerID, AdSize.Banner, AdPosition.Top);
            request = new AdRequest.Builder().Build();
        }

        if (TestAds)
		{
            bannerView = new BannerView("ca-app-pub-3940256099942544/6300978111", AdSize.Banner, AdPosition.Top);

            request = new AdRequest.Builder()
			.AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
			.AddTestDevice(SystemInfo.deviceUniqueIdentifier)  // My test device.
			.Build();
		}
		bannerView.LoadAd (request);
	}

	private void RequestInterstitial()
	{
		if (admanagerInstance.interstitialView != null)
		{
			admanagerInstance.interstitialView.Destroy();
		}

		// Create an empty ad request.

		AdRequest request = null;

        if (!TestAds)
        {
            admanagerInstance.interstitialView = new InterstitialAd(interstitialID);		//orignal
            request = new AdRequest.Builder().Build();
        }

        if (TestAds)
		{
            admanagerInstance.interstitialView = new InterstitialAd("ca-app-pub-3940256099942544/1033173712");        //test
            request = new AdRequest.Builder()
			.AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
			.AddTestDevice(SystemInfo.deviceUniqueIdentifier)  // My test device.
			.Build();						
		}
		admanagerInstance.interstitialView.LoadAd(request);
	}

	private void RequestRewardBasedVideo()
	{
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();

		// Load the rewarded video ad with the request.
		admanagerInstance.rewardBasedVideoAd.LoadAd(request, rewardVideoID);
	}
	#endregion


	#region AdmobRewardCallBackEvents
	public void HandleRewardBasedVideoLoaded(object sender, EventArgs args)
	{
		MonoBehaviour.print("HandleRewardBasedVideoLoaded event received");
	}

	public void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
	{
		MonoBehaviour.print(
			"HandleRewardBasedVideoFailedToLoad event received with message: "
			+ args.Message);
	}

	public void HandleRewardBasedVideoOpened(object sender, EventArgs args)
	{
		MonoBehaviour.print("HandleRewardBasedVideoOpened event received");
	}

	public void HandleRewardBasedVideoStarted(object sender, EventArgs args)
	{
		MonoBehaviour.print("HandleRewardBasedVideoStarted event received");
	}

	public void HandleRewardBasedVideoClosed(object sender, EventArgs args)
	{
		admanagerInstance.RequestRewardBasedVideo();

		MonoBehaviour.print("HandleRewardBasedVideoClosed event received");
	}

	//This is called when user completes Admob reward video
	public void HandleRewardBasedVideoRewarded(object sender, Reward args)
	{
		Debug.Log ("The ad was shown successfully");
	}

	public void HandleRewardBasedVideoLeftApplication(object sender, EventArgs args)
	{
		MonoBehaviour.print("HandleRewardBasedVideoLeftApplication event received");
	}
	#endregion 	//Reward Events Ends




	//Uity3D related Events
    /*
	public void ShowUnityVideoAd()
	{
		Debug.Log ("ShowUnityVideoAd");

		if (Advertisement.IsReady ("video"))
			Advertisement.Show ("video");
	}


	public void ShowUnityRewardVideo()
	{
		Debug.Log ("ShowUnityRewardVideo");
		if (Advertisement.IsReady ("rewardedVideo")) {
			Debug.Log ("Showing Advertisement");
			var options = new ShowOptions { resultCallback = HandleSHowResult };
			Advertisement.Show ("rewardedVideo",options);
		}
	}

	private void HandleSHowResult(ShowResult result)
	{
		switch (result) {

		case ShowResult.Finished:
			Debug.Log ("The Unity Reward ad was shown successfully");
			break;

		case ShowResult.Skipped:
			Debug.Log ("Ad was skipped");
			break;
		case ShowResult.Failed:
			Debug.LogError ("The ad fialed to be shown");
			break;
		}
	}
    */
}
