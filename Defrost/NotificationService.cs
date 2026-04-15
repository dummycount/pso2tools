using CommunityToolkit.WinUI.Behaviors;

namespace Pso2Tools.Defrost;

public class NotificationService
{
	public StackedNotificationsBehavior? NotificationQueue { get; set; }

	public void ShowNotification(Notification notification)
	{
		NotificationQueue?.Clear();
		NotificationQueue?.Show(notification);
	}
}
