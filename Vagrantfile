# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|
  config.vm.box = "ubuntu/xenial64"
  config.vm.provider "virtualbox" do |vb|
    vb.memory = "1024"
  end
  config.vm.network :forwarded_port, guest: 5673, host: 5673
  config.vm.provision "shell", inline: <<-SHELL
    apt-key adv --keyserver keys.gnupg.net --recv-keys 09617FD37CC06B54
    echo "deb https://dist.crystal-lang.org/apt crystal main" > /etc/apt/sources.list.d/crystal.list
    apt-get update
    apt-get install -y crystal --allow-unauthenticated
  SHELL
end
