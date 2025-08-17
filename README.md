# CTL-ALT-Elite
1. Install the following:
(for windows)
  - https://git-scm.com/downloads/win
  - https://desktop.github.com/download/
  - Unity 6.1 (6000.1.15f1)

2. In git terminal run:

  git lfs install

  git config --global core.autocrlf true
  
  git config --global core.safecrlf false
  
  git config --global core.longpaths true

This just gets a rid of the warnings that appear when you push, the warnings are because of the files that the ignore skips (it doesn't upload the core unity stuff because thats GBs)
this way it just has what we need to share to each other to see progress.


3. In GitHub Desktop: Clone the repo to a local folder (e.g Documents/GitHub/CTL-ALT-Elite).
4. In Unity Hub: open the repo folder, same as what you set the one in the last step.
5. Create a personal branch or pull the test branch

If you get a message saying the push is too large (>100mb) either the ignore isn't working in which case i had to use this in git:
   
    # Remove everything from tracking (but keep files locally)
    git rm -r --cached .
    
    # Re-add only allowed files, plus force-keep .gitignore & README.md
    git add . .gitignore README.md
    
    # Commit and push
    git commit -m "Apply fixed Unity .gitignore and remove ignored files from tracking"
    git push

Or use git lfs 
