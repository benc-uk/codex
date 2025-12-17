skill = 10
luck = 7
stamina = 20
bag = []

section cave {
  text "You stand at the entrance of a dark cave. The air is damp and the sound of dripping water echoes from within. There is moss and lichen growing on the walls, there is rubble and rocks on the ground. The cave extends into darkness ahead."
  
  vars {
    searched = false
  }

  option enter "Enter the cave cautiously." {
    goto dark_tunnel
  }

  option search "Search rocks" {
    condition not searched
    then searched = true
    trigger search_rocks
  }

  option go_out "Decide to stay outside and set up camp." {
    goto camp_outside
  }
}

section dark_tunnel {
  text "As you proceed deeper into the cave, the light from outside fades away. The tunnel narrows, and you can hear the distant sound of water dripping. Suddenly, you come across a fork in the path."

  option left_path "Take the left path." {
    goto left_path
  }

  option right_path "Take the right path." {
    goto right_path
  }
}

section camp_outside {
  text "You decide it's safer to stay outside the cave for now. You set up a small camp and prepare for the night, keeping an eye on the cave entrance."
}

section left_path {
  text "You take the left path, which leads you to a small underground stream. The sound of flowing water is soothing, and you feel a bit more at ease."

  option follow_stream "Follow the stream deeper into the cave." {
    goto placeholder
  }

  option return_fork "Return to the fork in the path." {
    goto dark_tunnel
  }
}

section right_path {
  text "You take the right path, which becomes increasingly narrow and steep. The air grows colder, and you can see your breath in front of you."

  option continue_climb "Continue climbing deeper into the cave." {
    goto placeholder
  }

  option return_fork "Return to the fork in the path." {
    goto dark_tunnel
  }
}

section placeholder {
  text "This is a placeholder section for future content."
}

event search_rocks {
  text "You carefully search through the rocks and rubble at the cave entrance. After a few minutes, you find a small, rusty dagger hidden among the stones. It might come in handy later."
  
  then insert bag "rusty dagger"

  option ok "OK" {
    goto cave
  }
}