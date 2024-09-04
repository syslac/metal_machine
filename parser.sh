#!/bin/zsh

# Original html to link-only
#cat setlist_list.html | grep "a href" | awk -F "\"" '{ print $2 }' | perl -pe 's/^../https:\/\/setlist.fm/' > setlist_list_link_only.txt

# Download saved links each into their separate file
#cat setlist_list_link_only.txt | while read i; do cd single_concerts; sleep 30; wget $i; cd ..; done;

echo "" > setlist_list_parsed.csv;
for file in single_concerts/*.html; do
  # get artist from file
  artist=`cat $file | grep qc:artist | awk -F "\"" '{ print $4 }'`;
  # get location from file
  location=`cat $file | grep qc:venue | awk -F "\"" '{ print $4 }'`;
  # get short location from file
  short_location=`cat $file | grep qc:venue | awk -F "\"" '{ print $4 }' | perl -pe 's/^.*?,\s*//'`;
  year=`cat $file | grep 'class="year"' | awk -F ">" '{ print $2 }' | awk -F "<" '{ print $1 }' | head -n 1`;
  month=`cat $file | grep 'class="month"' | awk -F ">" '{ print $2 }' | awk -F "<" '{ print $1 }' | head -n 1`;
  day=`cat $file | grep 'class="day"' | awk -F ">" '{ print $2 }' | awk -F "<" '{ print $1 }' | head -n 1`;

  echo "$artist;$location;$short_location;$year-$month-$day" >> setlist_list_parsed.csv;
done;

# html entities - add more as needed
perl -pi -e "s/\\&\\#039;/\\'/g" setlist_list_parsed.csv;
# months "look up table"
perl -pi -e "s/-Jan-/-01-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Feb-/-02-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Mar-/-03-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Apr-/-04-/g" setlist_list_parsed.csv;
perl -pi -e "s/-May-/-05-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Jun-/-06-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Jul-/-07-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Aug-/-08-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Sep-/-09-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Oct-/-10-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Nov-/-11-/g" setlist_list_parsed.csv;
perl -pi -e "s/-Dec-/-12-/g" setlist_list_parsed.csv;
# 2-digits format for days that had just 1
perl -pi -e 's/-(\d)$/-0$1/g' setlist_list_parsed.csv;

