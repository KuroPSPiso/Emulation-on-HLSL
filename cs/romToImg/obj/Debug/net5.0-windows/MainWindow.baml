   M S B A M L   `   `   `  ����C  ?romToImg, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:romToImgromToImg  5   	       romToImg.MainWindow   5   	   \ XPresentationFramework, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089W SPresentationCore, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=5.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=5.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:romToImg     5   	   -   5          � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      � .+�5
   
   �� -   5         ��Name$ lblFile��5
      $��filename��6!   $
��Left=�64   $��	60,88,0,0q�6O   $	��Top=�6b   �� 5   
   -   5         ��Name$ btnOpenFile��5      $
��Open��6&   $
��Left=�65   $��	60,63,0,0q�6P   $	��Top=�6c   6{   �� 5   
   -   5         ��Name$ 	imgResult��5      $
��Left=�6#   $	��203��6>   $��
60,114,0,0q�6K   $	��Top=�6_   $	��203��6w   �� 5   
   -   5        $ btnSave��5      $
��Save��6"   $
��Left=�61   $��
383,63,0,0q�6L   $	��Top=�6`   �� 5   
   -   5        $ 
lblProcess��5      $��waiting for file��6$   $
��Left=�6?   $��
383,88,0,0q�6Z   $	��Top=�6n   �� 5   
   -   5         ��Name$ slider��5      $
��Left=�6!   $��
129,65,0,0q�6<   $	��Top=�6P   $	��235��6h    � Minimum$ 0Q�6t    � Maximum$ 9Q�6�    �� IsSnapToTickEnabled$
 True��6�    �� TickFrequency$ 1Q�6�   6�   �� 5   
   -   5        $ lblResolution��5      $	��?/?��6'   $
��Left=�65   $��
129,88,0,0q�6P   $	��Top=�6d   5      5      