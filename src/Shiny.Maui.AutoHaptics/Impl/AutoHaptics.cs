namespace Shiny.Impl;

public class AutoHaptics : IAutoHaptics, IMauiInitializeService
{
    public bool Enabled { get; set; }
    public bool PageNavigationEnabled { get; set; }
    public bool ButtonClickEnabled { get; set; }


    public void Initialize(IServiceProvider services)
    {
        var appinject = services.GetRequiredService<IApplication>();
        if (appinject is not Application app)
            throw new ApplicationException("Auto Haptics not found");
        
        app.DescendantAdded += this.AppOnDescendantAdded;
        app.DescendantRemoved += this.AppOnDescendantRemoved;
    }
    

    void AppOnDescendantAdded(object? sender, ElementEventArgs e)
    {
        if (e.Element is Page page)
        {
            this.BindPage(page, true);
        }
        else if (e.Element is Button button)
        {
            
        }
    }
    
    
    void AppOnDescendantRemoved(object? sender, ElementEventArgs e)
    {
        if (e.Element is Page page)
        {
            this.BindPage(page, false);
        }
        else if (e.Element is VisualElement el)
        {
            if (el is Button button)
            {
                
            }
            else if (el is CarouselView carousel)
            {
                // carousel.Scrolled += 
            }
            else if (el is CollectionView cv)
            {
                // cv.Scrolled
            }
        }
    }


    void BindElement(VisualElement el, bool bind)
    {
        
    }

    void BindPage(Page page, bool bind)
    {
        page.NavigatedTo -= this.PageOnNavigatedTo;
        if (bind)
            page.NavigatedTo += this.PageOnNavigatedTo;
    }
    

    void PageOnNavigatedTo(object? sender, NavigatedToEventArgs e)
    {
        HapticFeedback.Default.Perform(HapticFeedbackType.Click);
    }
}